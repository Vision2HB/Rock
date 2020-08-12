using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName( "Location List" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Lists all the locations with their current check-in numbers" )]
    [IntegerField( "Refresh Interval (Seconds)", defaultValue: 30, key: "RefreshInterval" )]
    [LinkedPage( "Back Page")]
    [LinkedPage( "Location Detail Page")]
    [LinkedPage( "Evac Report Page" )]
    public partial class LocationList : Rock.Web.UI.RockBlock
    {

        #region ViewState

        private int _checkInTypeId
        {
            get
            {
                if ( ViewState["_checkInTypeId"] != null )
                {
                    return Convert.ToInt32( ViewState["_checkInTypeId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_checkInTypeId"] = value;
            }
        }

        private int _campusId
        {
            get
            {
                if ( ViewState["_campusId"] != null )
                {
                    return Convert.ToInt32( ViewState["_campusId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_campusId"] = value;
            }
        }

        private int _detailPageId
        {
            get
            {
                if ( ViewState["_detailPageId"] != null )
                {
                    return Convert.ToInt32( ViewState["_detailPageId"] );
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                ViewState["_detailPageId"] = value;
            }
        }

        private int _inroomId
        {
            get
            {
                if ( ViewState["_inroomId"] != null )
                {
                    return Convert.ToInt32( ViewState["_inroomId"] );
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                ViewState["_inroomId"] = value;
            }
        }

        private List<int> _groupLocationIds
        {
            get
            {
                if ( ViewState["_groupLocationIds"] != null )
                {
                    return ViewState["_groupLocationIds"].ToString().SplitDelimitedValues().AsIntegerList();
                }
                else
                {
                    return new List<int>();
                }
            }
            set
            {
                ViewState["_groupLocationIds"] = value.AsDelimited( "," );
            }
        }
        

        
        #endregion

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            timer.Interval = GetAttributeValue( "RefreshInterval" ).AsInteger() * 1000;
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            
            if ( !IsPostBack )
            {
                RockContext rockContext = new RockContext();
                //must include checkinTypeId and campusId
                _checkInTypeId = PageParameter( "CheckinTypeId" ).AsInteger();
                _campusId = PageParameter( "CampusId" ).AsInteger();

                _inroomId = new AttributeService( rockContext ).Get( "9C37D578-2BEA-4645-B36F-4975C42B56F1".AsGuid() ).Id;

                //create Location Detail link
                if ( GetAttributeValue( "LocationDetailPage" ).IsNotNullOrWhiteSpace() )
                {
                    _detailPageId = new PageService( rockContext ).Get( GetAttributeValue( "LocationDetailPage" ).AsGuid() ).Id;
                }

                //set Back button url
                if ( GetAttributeValue( "BackPage" ).IsNotNullOrWhiteSpace() )
                {
                    btnBack.PostBackUrl = "/page/" + new PageService( rockContext ).Get( GetAttributeValue( "BackPage" ).AsGuid() ).Id + "?CampusId=" + _campusId;
                }

                //load groupLocations
                var groupTypeIds = new GroupTypeService( rockContext ).GetChildGroupTypes( _checkInTypeId ).Select( t => t.Id ).ToList();
                var groups = new GroupService( rockContext ).Queryable( "GroupLocations" )
                    .Where( g => groupTypeIds.Contains( g.GroupTypeId ) && g.IsActive );
                _groupLocationIds = groups
                    .SelectMany( g => g.GroupLocations ).ToList().Where( gl => gl.Location.CampusId == _campusId && gl.Location.IsActive == true ).Select( gl => gl.Id ).ToList();
                
                
                //Bind Url to Evac button
                if ( GetAttributeValue( "EvacReportPage" ).IsNotNullOrWhiteSpace() )
                {
                    btnEvac.PostBackUrl = "/Page/" + new PageService( new RockContext() ).Get( GetAttributeValue( "EvacReportPage" ).AsGuid() ).Id + "?CheckinTypeId=" + _checkInTypeId + "&CampusId=" + _campusId ;
                }
            }

            BindGrid();
        }

        private class RoomResult
        {
            public string Area { get; set; }
            public int AreaOrder { get; set; }
            public int LocationId { get; set; }
            public string Location { get; set; }
            public int EnRoute { get; set; }
            public int InRoom { get; set; }
            public int CheckedOut { get; set; }
        }

        private void BindGrid()
        {
            

            List<RoomResult> roomResults = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationList." + _campusId.ToString() + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 30 ) ) as List<RoomResult>;
            
            gLocations.DataSource = roomResults;
            gLocations.DataBind();

            
        }

        private List<RoomResult> GetDataSet()
        {
            DateTime startDate = RockDateTime.Today;

            List<RoomResult> roomResults = new List<RoomResult>();

            try
            {
                var dataset = DbService.GetDataSet( @"

Declare @CheckInGroupTypeId int = " + _checkInTypeId.ToString() + @"
Declare @CurrentDate date = '" + startDate.ToString( "yyyy-MM-dd" ) + @"'
Declare @CampusId int = " + _campusId.ToString() + @"

--CTE for checkin group types and children gt areas
;WITH CTEgrouptypes AS (
	SELECT ChildGroupTypeId from [GroupTypeAssociation] GTA
	WHERE GTA.GroupTypeId = @CheckInGroupTypeId

	UNION ALL

	SELECT GTA.ChildGroupTypeId from [GroupTypeAssociation] GTA
	JOIN CTEgrouptypes on CTEgrouptypes.ChildGroupTypeId = GTA.GroupTypeId
)
--Select Campus Groups
Select G.Id, G.ParentGroupId, PG.Name as ParentGroup, G.GroupTypeId, G.Name
INTO #CampusGroups
From [Group] G
INNER JOIN [Group] PG ON G.ParentGroupId = PG.Id
Where G.CampusId = @CampusId
AND G.IsActive = 1 AND G.IsArchived != 1
AND G.GroupTypeId in ( Select ChildGroupTypeId FROM CTEgrouptypes )

--Select  group locations w attendances
SELECT
    PG.[Name] as [Area],
    L.Id as [LocationId],
    L.Name as [Location],
    CASE WHEN A.QualifierValueId IS NOT NULL THEN 1 ELSE 0 END as [IsInRoom],
    CASE WHEN A.EndDateTime IS NOT NULL THEN 1 ELSE 0 END as [IsCheckedOut],
    A.Id as [AttendanceId]
INTO #LocationAttendances
FROM
    GroupLocation GL
INNER JOIN
	[Group] G ON GL.GroupId = G.Id
LEFT JOIN 
	[Group] PG ON G.ParentGroupId = PG.Id
INNER JOIN
    [Location] L ON GL.LocationId = L.Id
LEFT JOIN
    [AttendanceOccurrence] AO ON AO.LocationId = L.Id AND AO.OccurrenceDate = @CurrentDate
LEFT JOIN
    [Attendance] A ON A.OccurrenceId = AO.Id  
WHERE
	GL.GroupId in ( Select Id from #CampusGroups )
	AND
    L.IsActive = 1
GROUP BY
	A.Id, A.EndDateTime, A.QualifierValueId, L.Name, L.Id, PG.[Name]

SELECT
    MAX(Area) as [Area],
    LocationId as [LocationId],
    Location as [Location],
    SUM( CASE WHEN IsInRoom != 1 AND IsCheckedOut != 1 AND AttendanceId IS NOT NULL THEN 1 ELSE 0 END ) as [EnRoute],
    SUM( CASE WHEN IsInRoom = 1 AND IsCheckedOut != 1 THEN 1 ELSE 0 END ) as [InRoom],
    SUM( CASE WHEN IsCheckedOut = 1 THEN 1 ELSE 0 END ) as [CheckedOut]
FROM
	#LocationAttendances
GROUP BY
    LocationId, Location
 ORDER BY
    Area, Location


DROP TABLE #CampusGroups
DROP TABLE #LocationAttendances
            ", CommandType.Text, null );

                roomResults = dataset.Tables[0].AsEnumerable().Select( dataRow => new RoomResult
                {
                    Area = dataRow.Field<string>( "Area" ),
                    AreaOrder = 0,
                    LocationId = dataRow.Field<int>( "LocationId" ),
                    Location = dataRow.Field<string>( "Location" ),
                    EnRoute = dataRow.Field<int>( "EnRoute" ),
                    InRoom = dataRow.Field<int>( "InRoom" ),
                    CheckedOut = dataRow.Field<int>( "CheckedOut" )

                } ).ToList();

            }
            catch
            {
                System.Threading.Thread.Sleep( 1500 );
                NavigateToCurrentPageReference();
            }

            return roomResults;
        }

        protected void gLocations_RowSelected( object sender, RowEventArgs e )
        {
            if ( e.RowKeyId > 0 )
            {
                var url = String.Empty;
                if ( _detailPageId > 0 && _checkInTypeId > 0 && e.RowKeyId > 0 /*&& ddlSchedule.SelectedValueAsInt() > 0 */)
                {
                    url = "/page/" + _detailPageId + "?CheckinTypeId=" + _checkInTypeId + "&LocationId=" + e.RowKeyId /*+ "&ScheduleId=" + ddlSchedule.SelectedValue*/;

                }

               
                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void timer_OnTick( object sender, EventArgs e )
        {
            // Grid is bound in OnLoad
        }
        
       
    }
}