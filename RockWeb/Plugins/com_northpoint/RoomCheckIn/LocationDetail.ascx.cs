using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using com_northpoint.KioskTypes.Model;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Communication = Rock.Model.Communication;
using FieldType = Rock.SystemGuid.FieldType;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName( "Location Detail" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Lists people checked into a location and allows check-in confirmation and editing" )]
    [IntegerField( "Refresh Interval (Seconds)", defaultValue: 10, key: "RefreshInterval" )]
    [IntegerField( "Note Type Id", "Note Type Id for Attendance records, if notes are to be shown/editable in details", required: false, key: "NoteTypeId")]
    [IntegerField( "Minutes Passed For New Attendance", "Allow app to create multiple attendances when checking out after original schedule has passed. Blank means no new attendances will be created", false, defaultValue: 10 )]
    [DefinedTypeField( "Attendance In Room Qualifier", "Defined Type that contains values to mark attendance as 'InRoom' (Value 1: InRoom)", required: true )]
    [LinkedPage("Location List Page", required: true)]
    [LinkedPage("Evac Report Page", required: false)]
    [CustomEnhancedListField("Check Out All By Schedule", "Allow a Check Out All by Schedule button for these CheckIn Configurations", @"
SELECT
    GT.Id as [Value],
    GT.Name as [Text]
FROM
    GroupType GT
INNER JOIN
    DefinedValue DV ON GT.GroupTypePurposeValueId = DV.Id
WHERE
    DV.Guid = CONVERT(UNIQUEIDENTIFIER, '" + Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE + @"')
", false )]
    [CustomEnhancedListField( "Staying Button", "Allow a Staying button on records to indicate they will stay in the room between schedules", @"
SELECT
    GT.Id as [Value],
    GT.Name as [Text]
FROM
    GroupType GT
INNER JOIN
    DefinedValue DV ON GT.GroupTypePurposeValueId = DV.Id
WHERE
    DV.Guid = CONVERT(UNIQUEIDENTIFIER, '" + Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE + @"')
", false )]
    public partial class LocationDetail : RockBlock
    {
        private DateTime _startDate = RockDateTime.Today;
        private bool _usesEnRoute = true;

        private const string EN_ROUTE = "En Route";
        private const string IN_ROOM = "In Room";
        private const string HEALTH_NOTES = "Health Notes";
        private const string CHECKED_OUT = "Checked Out";

        #region ViewState

        protected bool _textingEnabled
        {
            get
            {
                if ( ViewState["_textingEnabled"] != null )
                {
                    return Convert.ToBoolean( ViewState["_textingEnabled"] );
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["_textingEnabled"] = value;
            }
        }

        private int _checkInTypeId
        {
            get
            {
                if (ViewState["_checkInTypeId"] != null )
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

        private int _locationId
        {
            get
            {
                if ( ViewState["_locationId"] != null )
                {
                    return Convert.ToInt32( ViewState["_locationId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_locationId"] = value;
            }
        }


        private int _allergyId
        {
            get
            {
                if ( ViewState["_allergyId"] != null )
                {
                    return Convert.ToInt32( ViewState["_allergyId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_allergyId"] = value;
            }
        }

        private int _medicalId
        {
            get
            {
                if ( ViewState["_medicalId"] != null )
                {
                    return Convert.ToInt32( ViewState["_medicalId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_medicalId"] = value;
            }
        }

        private int _legalId
        {
            get
            {
                if ( ViewState["_legalId"] != null )
                {
                    return Convert.ToInt32( ViewState["_legalId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_legalId"] = value;
            }
        }

        private int _refusedId
        {
            get
            {
                if ( ViewState["_refusedId"] != null )
                {
                    return Convert.ToInt32( ViewState["_refusedId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_refusedId"] = value;
            }
        }

        protected string _roomListUrl
        {
            get
            {
                if ( ViewState["_roomListUrl"] != null )
                {
                    return Convert.ToString( ViewState["_roomListUrl"] );
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["_roomListUrl"] = value;
            }
        }

        protected string _evacUrl
        {
            get
            {
                if ( ViewState["_evacUrl"] != null )
                {
                    return Convert.ToString( ViewState["_evacUrl"] );
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["_evacUrl"] = value;
            }
        }

        protected string _locationName
        {
            get
            {
                if ( ViewState["_locationName"] != null )
                {
                    return Convert.ToString( ViewState["_locationName"] );
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["_locationName"] = value;
            }
        }

        private List<int> _groupIds
        {
            get
            {
                if ( ViewState["_groupIds"] != null )
                {
                    return ViewState["_groupIds"].ToString().SplitDelimitedValues().AsIntegerList();
                }
                else
                {
                    return new List<int>();
                }
            }
            set
            {
                ViewState["_groupIds"] = value.AsDelimited(",");
            }
        }

        private List<int> _allLocationIds
        {
            get
            {
                if ( ViewState["_allLocationIds"] != null )
                {
                    return ViewState["_allLocationIds"].ToString().SplitDelimitedValues().AsIntegerList();
                }
                else
                {
                    return new List<int>();
                }
            }
            set
            {
                ViewState["_allLocationIds"] = value.AsDelimited( "," );
            }
        }

        private int _inRoomQualifierValueId
        {
            get
            {
                if ( ViewState["_inRoomQualifierValueId"] != null )
                {
                    return Convert.ToInt32( ViewState["_inRoomQualifierValueId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_inRoomQualifierValueId"] = value;
            }
        }

        private int _stayingQualifierValueId
        {
            get
            {
                if ( ViewState["_stayingQualifierValueId"] != null )
                {
                    return Convert.ToInt32( ViewState["_stayingQualifierValueId"] );
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["_stayingQualifierValueId"] = value;
            }
        }


        #endregion

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/cordova-2.4.0.js", false );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            timer.Interval = GetAttributeValue( "RefreshInterval" ).AsInteger() * 1000;
        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            
            using ( var rockContext = new RockContext() )
            {
                
                
                //Set startDate on every page refresh
                _startDate = RockDateTime.Today;
                
                if ( !Page.IsPostBack )
                {
                    _checkInTypeId = PageParameter( "CheckinTypeId" ).AsInteger();
                    _locationId = PageParameter( "LocationId" ).AsInteger();

                    _locationName = "Room: " + new LocationService( rockContext ).Get( _locationId ).Name;

                    if ( string.IsNullOrWhiteSpace( hfCampusId.Value ) )
                    {
                        hfCampusId.Value = new LocationService( rockContext ).Get( _locationId ).CampusId.ToString();
                    }

                    if ( string.IsNullOrWhiteSpace( hfMessages.Value ) || string.IsNullOrWhiteSpace( hfFromNumber.Value ) )
                    {
                        var checkInConfig = new GroupTypeService( new RockContext() ).Get( _checkInTypeId );
                        checkInConfig.LoadAttributes();

                        // Populate the available messages
                        if ( string.IsNullOrWhiteSpace( hfMessages.Value ) && checkInConfig.GetAttributeValue( "com_northpoint.RoomCheckIn.SMSMessageTemplates" ).IsNotNullOrWhiteSpace() )
                        {
                            //Use text value, with delimited by "|"
                            hfMessages.Value = checkInConfig.GetAttributeValue( "com_northpoint.RoomCheckIn.SMSMessageTemplates" );
                        }

                        if ( string.IsNullOrWhiteSpace( hfFromNumber.Value ) && checkInConfig.AttributeValues.ContainsKey( "com_northpoint.RoomCheckIn.SMSFrom" ) )
                        {
                            var smsFromNumber = checkInConfig.AttributeValues["com_northpoint.RoomCheckIn.SMSFrom"].Value.AsGuidOrNull();

                            if ( smsFromNumber != null )
                            {
                                hfFromNumber.Value = DefinedValueCache.Get( smsFromNumber.Value ).Guid.ToString();
                            }
                        }
                    }

                    //find qualifier value ids
                    var attendanceInRoomQualifier = DefinedTypeCache.Get( GetAttributeValue( "AttendanceInRoomQualifier" ).AsGuid() );
                    if ( attendanceInRoomQualifier.IsNotNull() )
                    {
                        var inRoom = attendanceInRoomQualifier.DefinedValues.Where( dv => dv.Value == "InRoom" ).FirstOrDefault();
                        if ( inRoom.IsNotNull() )
                        {
                            _inRoomQualifierValueId = inRoom.Id;
                        }

                        var staying = attendanceInRoomQualifier.DefinedValues.Where( dv => dv.Value == "Staying" ).FirstOrDefault();
                        if ( staying.IsNotNull() )
                        {
                            _stayingQualifierValueId = staying.Id;
                        }
                    }
                    


                    //If Staying Button is not enabled, hide it
                    if ( GetAttributeValue( "StayingButton" ).IsNotNullOrWhiteSpace() && GetAttributeValue( "StayingButton" ).SplitDelimitedValues().AsIntegerList().Contains( _checkInTypeId ) )
                    {
                        hfStaying.Value = "True";
                    }
                    else
                    {
                        hfStaying.Value = "False";
                    }

                    //create list of groups that might use this location
                    var groupTypes = new GroupTypeService( rockContext ).GetChildGroupTypes( _checkInTypeId ).Select( t => t.Id );
                    var groups = new GroupService( rockContext ).Queryable( "GroupLocations" )
                        .Where( g => groupTypes.Contains( g.GroupTypeId ) && g.IsActive );
                    _groupIds = groups.Select( g => g.Id ).ToList();

                    //create list of all location Ids that might use this location for en route
                    _allLocationIds = groups
                        .SelectMany( g => g.GroupLocations ).ToList().Where( gl => gl.Location.CampusId == hfCampusId.ValueAsInt() ).Select( gl => gl.LocationId ).ToList();

                    // Only show SMS buttons if there is a from number and at least one predefined message
                    _textingEnabled = hfFromNumber.Value.IsNotNullOrWhiteSpace() && hfMessages.Value.IsNotNullOrWhiteSpace();

                    //Get attribute Ids
                    //_inroomId = AttributeCache.Get( "9C37D578-2BEA-4645-B36F-4975C42B56F1".AsGuid() ).Id;
                    _allergyId = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_ALLERGY.AsGuid() ).Id;
                    _medicalId = AttributeCache.Get( "63223b87-6ddb-4799-bce7-a849d5753b10".AsGuid() ).Id;
                    _legalId = AttributeCache.Get( "f832ab6f-b684-4eea-8db4-c54b895c79ed".AsGuid() ).Id;
                    _refusedId = AttributeCache.Get( "2551bbb8-1ca7-4b25-b494-307c6bb9f984".AsGuid() ).Id;

                    //Set RoomList button href from linked page in block attributes
                    if ( GetAttributeValue( "LocationListPage" ).IsNotNullOrWhiteSpace() )
                    {
                        _roomListUrl = "/page/" + new PageService( rockContext ).Get( GetAttributeValue( "LocationListPage" ).AsGuid() ).Id + "?CheckinTypeId=" + PageParameter( "CheckinTypeId" ) + "&CampusId=" + hfCampusId.Value;

                    }

                    //Set Evac Report button href from linked page in block attributes
                    if ( GetAttributeValue( "EvacReportPage" ).IsNotNullOrWhiteSpace() )
                    {
                        _evacUrl = "/page/" + new PageService( rockContext ).Get( GetAttributeValue( "EvacReportPage" ).AsGuid() ).Id + "?CheckinTypeId=" + PageParameter( "CheckinTypeId" ) + "&CampusId=" + hfCampusId.Value /*+ "&ScheduleId=" + scheduleId*/ + "&LocationId=" + _locationId;
                    }
                    

                    //Start off on en_route tab
                    hfActiveTab.Value = EN_ROUTE;
                    BindEnRouteGrid();
                }
            }
            
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            var activeTab = hfActiveTab.Value;
            ShowHideTab( activeTab == EN_ROUTE || ( activeTab == string.Empty && _usesEnRoute ), liEnRoute );
            ShowHideTab( activeTab == EN_ROUTE || ( activeTab == string.Empty && _usesEnRoute ), divEnRoute );
            ShowHideTab( activeTab == IN_ROOM || ( activeTab == string.Empty && !_usesEnRoute ), liInRoom );
            ShowHideTab( activeTab == IN_ROOM || ( activeTab == string.Empty && !_usesEnRoute ), divInRoom );
            ShowHideTab( activeTab == HEALTH_NOTES, liHealthNotes );
            ShowHideTab( activeTab == HEALTH_NOTES, divHealthNotes );
            ShowHideTab( activeTab == CHECKED_OUT, liCheckedOut );
            ShowHideTab( activeTab == CHECKED_OUT, divCheckedOut );

            // Don't refresh the screen while the dialog is open, it loses focus in dropdown menus
            timer.Enabled = string.IsNullOrEmpty( hfActiveDialog.Value );
        }

        protected void pillEnRoute_Click( object sender, EventArgs e )
        {
            hfActiveTab.Value = EN_ROUTE;
            BindEnRouteGrid();
        }

        protected void pillInRoom_Click( object sender, EventArgs e )
        {
            hfActiveTab.Value = IN_ROOM;
            BindInRoomGrid();
        }

        protected void pillHealthNotes_Click( object sender, EventArgs e )
        {
            hfActiveTab.Value = HEALTH_NOTES;
            BindHealthNotesGrid();
        }

        protected void pillCheckedOut_Click( object sender, EventArgs e )
        {
            hfActiveTab.Value = CHECKED_OUT;
            BindCheckedOutGrid();
        }

        private void ShowHideTab( bool show, HtmlGenericControl control )
        {
            if ( show )
            {
                control.AddCssClass( "active" );
            }
            else
            {
                control.RemoveCssClass( "active" );
            }
        }

        private class CheckInsResults
        {
            public int PersonId;
            public int? PersonAliasId;
            public string Name;
            public int AttendanceId;
            public DateTime StartDateTime;
            public DateTime? EndDateTime;
            public int? ScheduleId;
            public string Schedule;
            public string Code;
            public int? GroupId;
            public string Location;
            public int? LocationId;
            public string Group;
            public int GroupTypeId;
            public bool? InRoom;
            public bool? Staying;
            public string Allergy;
            public string SpecialNote;
        }

        /// <summary>
        /// The standard query
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<CheckInsResults> CheckIns( RockContext rockContext )
        {
            
            List<CheckInsResults> checkinResults = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
            
            
            //Refresh badge numbers
            var enroute = ( from c in checkinResults where c.InRoom != true && c.EndDateTime == null && c.LocationId == _locationId select c.AttendanceId ).Count();
            var inRoom = ( from c in checkinResults where c.InRoom == true && c.EndDateTime == null && c.LocationId == _locationId select c.AttendanceId ).Count();
            var checkout = ( from c in checkinResults where  c.EndDateTime != null && c.LocationId == _locationId select c.AttendanceId ).Count();

            lLocation.Text = ( _locationName.PadRight( 25, ' ' ) + inRoom.ToStringSafe() +" In Room" ).Replace( " ", "&nbsp;" );

            pillEnRoute.Text = " En Route <span class='badge'>" + enroute.ToStringSafe() + "</span>";
            pillInRoom.Text = " In Room <span class='badge'>" + inRoom.ToStringSafe() + "</span>";
            pillCheckedOut.Text = " Checked Out <span class='badge'>" + checkout.ToStringSafe() + "</span>";

            return checkinResults;
                
        }

        private List<CheckInsResults> GetDataSet( )
        {
            List<CheckInsResults> checkinResults = new List<CheckInsResults>();
            try
            {
                DateTime startDate = RockDateTime.Today;
                var dataset = DbService.GetDataSet( @"
Declare @CheckInGroupTypeId int = " + _checkInTypeId.ToString() + @"
Declare @CurrentDate date = '" + startDate.ToString( "yyyy-MM-dd" ) + @"'
Declare @CampusId int = " + hfCampusId.Value + @"

;WITH CTEgrouptypes AS (
	SELECT ChildGroupTypeId from [GroupTypeAssociation] GTA
	WHERE GTA.GroupTypeId = @CheckInGroupTypeId

	UNION ALL

	SELECT GTA.ChildGroupTypeId from [GroupTypeAssociation] GTA
	JOIN CTEgrouptypes on CTEgrouptypes.ChildGroupTypeId = GTA.GroupTypeId
)

Select G.Id, G.ParentGroupId, PG.Name as ParentGroup, G.GroupTypeId, G.Name
INTO #CampusGroups
From [Group] G
INNER JOIN [Group] PG ON G.ParentGroupId = PG.Id
Where G.CampusId = @CampusId
AND G.IsActive = 1 AND G.IsArchived != 1
AND G.GroupTypeId in ( Select ChildGroupTypeId FROM CTEgrouptypes )

--SELECT * FROM #CampusGroups

Select A.PersonAliasId, A.Id, AC.Code, A.StartDateTime, A.EndDateTime, A.QualifierValueId, A.OccurrenceId, AO.GroupId, AO.LocationId, L.Name as [Location], AO.ScheduleId, S.Name as Schedule, AO.OccurrenceDate
INTO #Attendances
FROM [AttendanceOccurrence] AO
INNER JOIN [Attendance] A ON A.OccurrenceId = AO.Id
LEFT JOIN [Location] L ON AO.LocationId = L.Id
LEFT JOIN [Schedule] S ON AO.ScheduleId = S.Id
LEFT JOIN [AttendanceCode] AC ON A.AttendanceCodeId = AC.Id
WHERE AO.GroupId in ( Select Id from #CampusGroups )
AND AO.OccurrenceDate = @CurrentDate

--SELECT * FROM #Attendances

Select PA.Id, PA.PersonId, P.LastName, P.NickName, P.FirstName, P.DaysUntilBirthday, ALLERGY.Value as Allergy, MEDICAL.Value as Medical, LEGAL.Value as Legal, REFUSED.Value as Refused
INTO #People
FROM [PersonAlias] PA
INNER JOIN [Person] P ON PA.PersonId = P.Id
LEFT JOIN
    AttributeValue ALLERGY ON ALLERGY.AttributeId = " + _allergyId.ToString() + @" AND ALLERGY.EntityId = P.Id
 LEFT JOIN
    AttributeValue MEDICAL ON MEDICAL.AttributeId = " + _medicalId.ToString() + @" AND MEDICAL.EntityId = P.Id
 LEFT JOIN
    AttributeValue LEGAL ON LEGAL.AttributeId = " + _legalId.ToString() + @" AND LEGAL.EntityId = P.Id
 LEFT JOIN
    AttributeValue REFUSED ON REFUSED.AttributeId = " + _refusedId.ToString() + @" AND REFUSED.EntityId = P.Id
WHERE PA.Id in ( SELECT PersonAliasId from #Attendances )

--SELECT * FROM #People

SELECT
    P.PersonId as PersonId,
    P.Id as PersonAliasId,
    ISNULL(P.NickName,P.FirstName) + ' ' + P.LastName as [Name],
    ISNULL(A.Code, '') as [Code],
    A.Id as AttendanceId,
    A.StartDateTime as StartDateTime,
    A.EndDateTime as EndDateTime,
    A.ScheduleId as ScheduleId,
    A.Schedule as Schedule,
    A.GroupId as GroupId,
    CG.Name as [Group],
    CG.ParentGroup as [ParentGroup],
    A.[Location] as [Location],
    A.LocationId as LocationId,
    CG.GroupTypeId as [GroupTypeId],
    CASE WHEN A.QualifierValueId IS NOT NULL THEN 1 ELSE 0 END as [InRoom],
    CASE WHEN A.QualifierValueId = 2868 THEN 1 ELSE 0 END as [Staying],
    CASE WHEN ISNULL(P.Allergy,'') != '' THEN P.Allergy ELSE '' END + CASE WHEN ISNULL(P.Medical,'') != '' THEN ' --Medical:' + P.Medical ELSE '' END + CASE WHEN ISNULL(P.Legal,'') != '' THEN ' --Legal:' + P.Legal ELSE '' END as [Allergy],
    CASE WHEN ( P.DaysUntilBirthday < 7 OR P.DaysUntilBirthday > 358 ) THEN '<i class=''fa fa-birthday-cake''></i> ' END as [Birthday],
    ( CASE WHEN EXISTS(
        SELECT 1 FROM Attendance A1
        INNER JOIN AttendanceOccurrence AO1 ON A1.OccurrenceId = AO1.Id
        INNER JOIN [Group] G1 ON AO1.GroupId = G1.Id
        WHERE A1.PersonAliasId = P.Id
        AND G1.GroupTypeId = CG.GroupTypeId
        AND AO1.OccurrenceDate != @CurrentDate
        ) THEN '' ELSE ' VIP' END ) AS [VIP],
    CASE WHEN ISNULL(P.Allergy,'') != '' OR ISNULL(P.Medical,'') != '' OR ISNULL(P.Legal,'') != '' THEN '<i class=''fa fa-exclamation-triangle''></i> ' END as [Alert],
    CASE WHEN ISNULL(P.Refused,'') = 'True' THEN '<i class=''fa fa-camera''></i> ' END as [Refused]
        
 FROM
	#CampusGroups CG
INNER JOIN
	#Attendances A ON A.GroupId = CG.Id
INNER JOIN
	#People P ON A.PersonAliasId = P.Id
ORDER BY P.LastName, ISNULL(P.NickName, P.FirstName), A.Code, A.Id


DROP TABLE #CampusGroups
DROP TABLE #Attendances
DROP TABLE #People
    
    
            ", CommandType.Text, null );

                checkinResults = dataset.Tables[0].AsEnumerable().Select( dataRow => new CheckInsResults
                {
                    PersonId = dataRow.Field<int>( "PersonId" ),
                    PersonAliasId = dataRow.Field<int>( "PersonAliasId" ),
                    Name = dataRow.Field<string>( "Name" ),
                    Code = dataRow.Field<string>( "Code" ),
                    AttendanceId = dataRow.Field<int>( "AttendanceId" ),
                    StartDateTime = dataRow.Field<DateTime>( "StartDateTime" ),
                    EndDateTime = dataRow.Field<DateTime?>( "EndDateTime" ),
                    ScheduleId = dataRow.Field<int?>( "ScheduleId" ) ?? -1,
                    Schedule = dataRow.Field<string>( "Schedule" ),
                    GroupId = dataRow.Field<int?>( "GroupId" ) ?? -1,
                    Group = dataRow.Field<string>( "Group" ),
                    Location = dataRow.Field<string>( "Location" ),
                    LocationId = dataRow.Field<int?>( "LocationId" ) ?? -1,
                    GroupTypeId = dataRow.Field<int?>( "GroupTypeId" ) ?? -1,
                    InRoom = dataRow.Field<int?>( "InRoom" ) == 1,
                    Staying = dataRow.Field<int?>( "Staying" ) == 1,
                    Allergy = dataRow.Field<string>( "Allergy" ),
                    SpecialNote = dataRow.Field<string>( "Birthday" ) + dataRow.Field<string>( "VIP" ) + dataRow.Field<string>( "Alert" ) + dataRow.Field<string>( "Refused" )
                } ).ToList();

            }
            catch
            {
                //System.Threading.Thread.Sleep( 1500 ); // May be causing multiple pages to sleep?
                NavigateToCurrentPageReference();
            }

            return checkinResults;
        }

        private void BindEnRouteGrid()
        {
            if ( hfActiveTab.Value == EN_ROUTE ) 
            {
                using ( var rockContext = new RockContext() )
                {
                    //var enRouteId = hfEnRouteId.Value.AsInteger();

                    var checkIns = CheckIns( rockContext );

                    // All people who are en route for the selected schedule and eligible to come in this room
                    var dataSource = ( from c in checkIns
                                           //join gl in new GroupLocationService( rockContext ).Queryable().AsNoTracking() on c.GroupId equals gl.GroupId
                                       where c.InRoom != true
                                             //&& c.LocationId == _locationId
                                             && c.EndDateTime == null
                                       /*&& !checkIns.Any( s => s.PersonId == c.PersonId && s.StartDateTime > c.StartDateTime )*/ // Exclude any people that have check ins later in the same day on this schedule
                                       orderby ( c.LocationId == _locationId ? 0 : 1 ), c.Name, c.Code, c.AttendanceId
                                       select new
                                       {
                                           Id = c.AttendanceId
                                                , c.PersonId
                                                , c.Name
                                                , Group = c.LocationId == _locationId ? c.Group : "Room #: " + c.Location + " - " + c.Group
                                                , c.Code
                                                , c.GroupTypeId
                                                , c.GroupId
                                                , c.LocationId
                                                , c.SpecialNote
                                                , c.ScheduleId
                                                , c.Schedule
                                       } );

                    gEnRoute.DataSource = dataSource.Where( a => a.LocationId == _locationId ).ToList();
                    gEnRoute.DataBind();

                    //show checkout ALL buttons, one for each schedule appearing
                    if ( GetAttributeValue( "CheckOutAllBySchedule" ).IsNotNullOrWhiteSpace() && GetAttributeValue( "CheckOutAllBySchedule" ).SplitDelimitedValues().AsIntegerList().Contains( _checkInTypeId ) )
                    {
                        //var scheduleList = new ScheduleService( rockContext ).GetByIds( dataSource.Where( a => a.LocationId == _locationId ).Where( c => c.ScheduleId.HasValue ).Select( c => c.ScheduleId.Value ).ToList() ).ToList();
                        var scheduleList = dataSource.Where( a => a.LocationId == _locationId ).Select( d => new { Id = d.ScheduleId, Name = d.Schedule } ).Distinct().ToList();
                        rptEnRouteDismiss.DataSource = scheduleList;
                        rptEnRouteDismiss.DataBind();
                    }


                    //bind other and limit by search text
                    dataSource = dataSource.Where( a => a.LocationId != _locationId );

                    if ( tbSearchOther.Text.IsNotNullOrWhiteSpace() )
                    {
                        dataSource = dataSource.Where( a => a.Name.StartsWith( tbSearchOther.Text.Trim() ) || a.Code.StartsWith( tbSearchOther.Text.Trim() ) );
                    }
                        

                    gEnRouteOther.DataSource = dataSource.ToList();
                    gEnRouteOther.DataBind();


                    
                }
            }
        }

        private void BindInRoomGrid()
        {
            if ( hfActiveTab.Value == IN_ROOM )
            {
                using ( var rockContext = new RockContext() )
                {
                    var checkIns = CheckIns( rockContext );
                    //_stayingIds = string.IsNullOrWhiteSpace( Session["StayingIds"] as string ) ? new List<int>() : Session["StayingIds"].ToString().SplitDelimitedValues().AsIntegerList();

                    // All people who are checked into this room in the selected schedule 
                    var dataSource = ( from c in checkIns
                                       where c.LocationId == _locationId
                                             && c.InRoom == true
                                             && c.EndDateTime == null
                                             /*&& !checkIns.Any( s => s.PersonId == c.PersonId && s.StartDateTime > c.StartDateTime )*/ // Exclude any people that have check ins later in the same day on this schedule
                                       orderby c.Name, c.Code, c.AttendanceId
                                       select new
                                              {
                                                  Id = c.AttendanceId
                                                , c.PersonId
                                                , c.Name
                                                , c.Group
                                                , c.Code
                                                , c.GroupTypeId
                                                , c.GroupId
                                                , c.ScheduleId
                                                , c.Schedule
                                                , c.SpecialNote
                                                , Staying = ( c.Staying == true ? "btn-warning" : "btn-default" )
                                       } ).ToList();

                    

                    gInRoom.DataSource = dataSource;
                    gInRoom.DataBind();

                    //show checkout ALL buttons, one for each schedule appearing
                    if ( GetAttributeValue( "CheckOutAllBySchedule" ).IsNotNullOrWhiteSpace() && GetAttributeValue( "CheckOutAllBySchedule" ).SplitDelimitedValues().AsIntegerList().Contains( _checkInTypeId ) )
                    {
                        //var scheduleList = new ScheduleService( rockContext ).GetByIds( dataSource.Where( c => c.ScheduleId.HasValue ).Select( c => c.ScheduleId.Value ).ToList() ).ToList();
                        var scheduleList = dataSource.Select( d => new { Id = d.ScheduleId, Name = d.Schedule } ).Distinct().ToList();
                        rptCheckOutBySchedule.DataSource = scheduleList;
                        rptCheckOutBySchedule.DataBind();
                    }
                    
                    
                }
            }
        }

        private void BindHealthNotesGrid()
        {
            if ( hfActiveTab.Value == HEALTH_NOTES )
            {
                using ( var rockContext = new RockContext() )
                {
                    var checkIns = CheckIns( rockContext );
                    

                    var dataSource = checkIns.Where( c => c.Allergy.IsNotNullOrWhiteSpace() )
                        .Where( c => c.LocationId == _locationId )
                        .Where( c => c.InRoom == true )
                        .Where( c => c.EndDateTime == null )
                        .Select( c => new
                        {
                            Id = c.AttendanceId,
                            c.PersonId, 
                            c.Name, 
                            c.Group,
                            c.Code,
                            c.GroupTypeId,
                            c.GroupId,
                            Note = c.Allergy,
                            c.SpecialNote
                        }).ToList();

                    gHealthNotes.DataSource = dataSource;
                    gHealthNotes.DataBind();
                }
            }
        }

        protected void gHealthNotes_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindHealthNotesGrid();
        }

        private void BindCheckedOutGrid()
        {
            if ( hfActiveTab.Value == CHECKED_OUT )
            {
                using ( var rockContext = new RockContext() )
                {
                    var checkIns = CheckIns( rockContext );

                    var dataSource = ( from c in checkIns
                                       //join gl in new GroupLocationService( rockContext ).Queryable().AsNoTracking() on c.GroupId equals gl.GroupId
                                       where c.EndDateTime != null
                                             && c.LocationId == _locationId // Checked into a group associated with this room
                                             /*&& !checkIns.Any( s => s.PersonId == c.PersonId && s.StartDateTime > c.StartDateTime )*/ // Exclude any people that have check ins later in the same day on this schedule
                                       orderby c.Name, c.Code, c.AttendanceId
                                       select new
                                              {
                                                  Id = c.AttendanceId
                                                , c.PersonId
                                                , c.Name
                                                , c.Group
                                                , c.Code
                                                , c.GroupTypeId
                                                , c.GroupId
                                                , c.SpecialNote
                                              } ).ToList();

                    gCheckedOut.DataSource = dataSource;
                    gCheckedOut.DataBind();
                }
            }
        }

        protected void gEnRoute_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindEnRouteGrid();
        }

        public class Adult
        {
            public int ID;
            public int PersonAliasId;
            public string Name { get; set; }
            public ICollection<PhoneNumber> PhoneNumbers;
            public int Sequence;
            public string Caption { get; set; }

            public List<PhoneNumber> PhoneNumbersOrdered
            {
                get
                {
                    // Eliminate duplicate numbers that show up higher in the list
                    var numbers = new List<PhoneNumber>();
                    foreach ( var number in PhoneNumbers.OrderBy( p => p.NumberTypeValue.Value == "Mobile" ? 0 : 1 ).ThenBy( p => p.NumberTypeValue.Value == "Home" ? 0 : 1 ).ToList() )
                    {
                        if ( numbers.All( x => x.NumberFormatted != number.NumberFormatted ) )
                        {
                            numbers.Add( number );
                        }
                    }

                    return numbers;
                }
            }
            
        }

        private void ShowSMSDialog(string personAliasId, string phoneNumber)
        {
            hfSMSAliasId.Value = personAliasId;
            hfPhoneNumber.Value = phoneNumber;

            var personId = hfPersonId.Value.AsIntegerOrNull();

            if ( personId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var ministry = GetMinistryName();
                    var person = new PersonService( rockContext ).Get( personId.Value );
                    var pronoun = person.Gender == Gender.Male ? "him" : person.Gender == Gender.Female ? "her" : "them";
                    var recipient = new PersonAliasService( rockContext ).GetPerson( personAliasId.AsInteger() );

                    rMessages.DataSource = hfMessages.Value.Split( '|' )
                                                     .Select( m => m.Replace( "{recipient}", recipient.NickName )
                                                                    .Replace( "{ministry}", ministry )
                                                                    .Replace( "{name}", person.NickName )
                                                                    .Replace( "{pronoun}", pronoun )
                                                            );
                    rMessages.DataBind();
                }
            }

            ShowDialog( "SMS" );
        }

        private string GetMinistryName()
        {
            var groupTypeService = new GroupTypeService( new RockContext() );
            var checkInConfig = groupTypeService.Get( _checkInTypeId );
            checkInConfig.LoadAttributes();

            var ministry = string.Empty;

            if ( checkInConfig.AttributeValues.ContainsKey( "com_northpoint.RoomCheckIn.DefaultMinistryName" ) )
            {
                ministry = checkInConfig.AttributeValues["com_northpoint.RoomCheckIn.DefaultMinistryName"].Value;
            }

            var groupTypeId = hfGroupTypeId.Value.AsInteger();

            if ( groupTypeId != 0 )
            {
                var groupType = groupTypeService.Get( groupTypeId );
                groupType.LoadAttributes();

                if ( groupType.AttributeValues.ContainsKey( "com_northpoint.RoomCheckIn.MinistryName" ) )
                {
                    var ministryName = groupType.AttributeValues["com_northpoint.RoomCheckIn.MinistryName"].Value;

                    if ( ministryName.IsNotNullOrWhiteSpace() )
                    {
                        ministry = ministryName;
                    }
                }
            }

            return ministry;
        }

        private void ShowEditDialog( RowEventArgs e, bool editGroups = true )
        {

            AllowEditsSMS( false ); //hide/show edit abilities
            pnlPIN.Visible = false; //will be enabled by button
            btnShowEdit.Visible = true; //button to show PIN

            //ddlArea.Enabled = editGroups;
            //ddlSmallGroup.Enabled = editGroups;

            var attendanceId = ( int ) e.RowKeyValues[0];
            var personId = ( int ) e.RowKeyValues[1];
            var groupId = ( int ) e.RowKeyValues[2];
            var groupTypeId = ( int ) e.RowKeyValues[3];

            hfAttendanceId.Value = attendanceId.ToString();
            hfPersonId.Value = personId.ToString();
            hfGroupId.Value = groupId.ToString();
            hfGroupTypeId.Value = groupTypeId.ToString();

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( personId );
                person.LoadAttributes();
                tbNickName.Text = person.NickName;
                dpBirthdate.SelectedDate = person.BirthDate;
                tbHealthNote.Text = person.AttributeValues["Arena-16-81"].Value;
                tbLegalNote.Text = person.AttributeValues["LegalNotes"].Value;
                tbAllergy.Text = person.AttributeValues["Allergy"].Value;

                tbNotes.Text = string.Empty;
               

                //Notes: Parent Location
                tbParentLocation.Label = "Parent Location";
                if( person.GetAttributeValue( "com_northpoint.RoomCheckIn.LastParentLocation" ).IsNotNullOrWhiteSpace() )
                {
                    tbParentLocation.Visible = true;
                    tbParentLocation.Text = person.GetAttributeValue( "com_northpoint.RoomCheckIn.LastParentLocation" );
                }
                else
                {
                    tbParentLocation.Visible = false;
                }
                

                var areaDataSource = new GroupService( rockContext ).GetByIds( _groupIds ).ToList()
                    .Where( g => g.CampusId == hfCampusId.Value.AsInteger() )
                    .Select( g => g.ParentGroup ?? new Rock.Model.Group { Id = 0, Name = "No Parent Group" } )
                    .DistinctBy( p => p.Id ).ToList();

                areaDataSource.Add( new Rock.Model.Group { Id = -1, Name = "" } );


                //ddlArea.DataSource = areaDataSource;
                //ddlArea.DataBind();

                ////Set default group
                //if( hfGroupId.ValueAsInt() > 0 )
                //{
                //    ddlArea.SelectedValue =  ( new GroupService( rockContext ).Get( hfGroupId.ValueAsInt() ).ParentGroupId ?? -1 ).ToString();
                //}


                //BindSmallGroupDDL();

                BindParents(rockContext, person, attendanceId);
            }
            

            // Printer list
            ddlKiosk.Items.Clear();
            //ddlKiosk.DataSource = DbService.GetDataTable( 
            //    "wcUtil_CheckInPrinters", 
            //    CommandType.StoredProcedure,
            //    new Dictionary<string, object> {{"CheckInTemplateId", _checkInTypeId}} 
            //    );

            //Get Printers in Devices that have a location of the selected Campus (does not iterate through parent locations to find campus, must be assigned campus directly)
            var printers = new DeviceService( new RockContext() ).GetByDeviceTypeGuid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER.AsGuid() ).ToList();

            //Filter down printers by Campus
            printers = printers.Where( p => p.Locations.Any( l => l.CampusId == hfCampusId.Value.AsIntegerOrNull() ) ).ToList();

            var listPrinters = new List<Device>();

            //Add Local Printer with 0 as ID
            //listPrinters.Add( new Device { Name = "Local Print", Id = 0, PrintFrom = Rock.Model.PrintFrom.Client } );

            listPrinters.AddRange( printers );
            
            ddlKiosk.DataSource = listPrinters.Select( d => new { d.Id, d.Name } ).ToList();
            ddlKiosk.DataBind();
            ddlKiosk.Items.Insert( 0, new ListItem( None.Text, "-1" ) );

            var selectedPrinter = Session["LocationDetail_SelectedPrinter"];
            
            if (selectedPrinter != null && ddlKiosk.Items.FindByValue(selectedPrinter.ToString()) != null)
            {
                ddlKiosk.SelectedValue = selectedPrinter.ToString();
            }
            else if ( Request.Cookies["com_northpoint.KioskType.KioskName"] != null && Request.Cookies["com_northpoint.KioskType.KioskName"].Value.IsNotNullOrWhiteSpace() )  //Find selected kiosk printer if available
            {
                var kiosk = new KioskService( new RockContext() ).Get( Request.Cookies["com_northpoint.KioskType.KioskName"].Value.AsInteger() );
                if ( kiosk != null && listPrinters.Any( p => p.Id == kiosk.PrinterDeviceId ) )
                {
                    ddlKiosk.SelectedValue = kiosk.PrinterDeviceId.ToString();
                }
            }
            
            btnPrint.Enabled = ( ddlKiosk.SelectedValue.AsIntegerOrNull() >= 0 );

            ShowDialog( "Edit" );
        }

        protected void gEnRoute_Edit( object sender, RowEventArgs e )
        {
            ShowEditDialog( e );
        }

        protected void gInRoom_Edit( object sender, RowEventArgs e )
        {
            ShowEditDialog( e );
        }

        protected void gHealthNotes_Edit( object sender, RowEventArgs e )
        {
            ShowEditDialog( e );
        }

        protected void gCheckedOut_Edit( object sender, RowEventArgs e )
        {
            ShowEditDialog( e, false );
        }

        //protected void ddlArea_SelectedIndexChanged( object sender, EventArgs e )
        //{
        //    BindSmallGroupDDL();
        //}

        //private void BindSmallGroupDDL()
        //{
        //    using ( var rockContext = new RockContext() )
        //    {
        //        var dsSmallGroup = new GroupService( rockContext ).GetByIds( _groupIds ).ToList()
        //            .Where( g => g.GroupLocations.Any( gl => /*gl.Schedules.Any( s => s.Id == _scheduleId ) &&*/ _allLocationIds.Contains(gl.LocationId) ) )  //Limit to small groups that have a current schedule
        //            //.Where( g => g.CampusId == hfCampusId.Value.AsInteger() )
        //            .Where( g => ( g.ParentGroupId ?? -1 ) == ddlArea.SelectedValueAsId() ).ToList();

        //        //Add Blank Group
        //        dsSmallGroup.Add( new Rock.Model.Group { Id = -1, Name = "" } );

        //        // Add the default group from the current schedule to the top of the list
        //        //foreach (var gt in groupType.ChildGroupTypes)
        //        //{
        //        //    dsSmallGroup.InsertRange( 0, gt.Groups.Where( g => g.IsActive && g.GroupLocations.Any( l => l.Schedules.Any( s => s.Id == _scheduleId ) ) ) );
        //        //}

        //        ddlSmallGroup.DataSource = dsSmallGroup;
        //        ddlSmallGroup.DataBind();

        //        var groupId = hfGroupId.Value.AsIntegerOrNull();
        //        if ( groupId.HasValue && dsSmallGroup.Any( x => x.Id == groupId.Value ) )
        //        {
        //            ddlSmallGroup.SelectedValue = groupId.Value.ToString();
        //        }
        //        else
        //        {
        //            //Select blank
        //            ddlSmallGroup.SelectedValue = "-1";
        //        }
        //    }
        //}

        private void BindParents(RockContext rockContext, Person person, int attendanceId)
        {
            var attendanceService = new AttendanceService( rockContext );
            var attendance = attendanceService.GetNoTracking( attendanceId );
            
            // Adults that checked in the child
            List<Adult> checkInAdults = new List<Adult>();
            if ( attendance.SearchResultGroup.IsNotNull() )
            {
                checkInAdults = attendance.SearchResultGroup.Members
                    .Where( x => x.Person.GetFamilyRole( rockContext ).Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()
                                 && x.Person.PhoneNumbers.Any() )
                    .Select( x => new Adult
                    {
                        ID = x.PersonId,
                        PersonAliasId = x.Person.PrimaryAliasId.Value,
                        Name = x.Person.FullName,
                        PhoneNumbers = x.Person.PhoneNumbers,
                        Sequence = 1,
                        Caption = ""
                    } )
                    .ToList();
            }


            // Adults in the child's family who are not already in the list
            var parents = person.GetFamilyMembers( false, rockContext )
                                .ToList()
                                .Where( x => x.Person.GetFamilyRole( rockContext ).Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()
                                             && x.Person.PhoneNumbers.Any()
                                             && checkInAdults.All( a => a.ID != x.PersonId )
                                      )
                                .Select( x => new Adult
                                {
                                    ID = x.PersonId,
                                    PersonAliasId = x.Person.PrimaryAliasId.Value,
                                    Name = x.Person.FullName,
                                    PhoneNumbers = x.Person.PhoneNumbers,
                                    Sequence = 2,
                                    Caption = " <small>(Parent)</small>"
                                } )
                                .ToList();

            var adults = parents.Concat( checkInAdults ).OrderBy( x => x.Sequence ).ThenBy( x => x.ID ).ToList();

            rAdults.DataSource = adults;
            rAdults.DataBind();
        }

        private void Save()
        {
            var personId = hfPersonId.Value.AsIntegerOrNull();
            if ( personId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    var person = personService.Get( personId.Value );
                    person.LoadAttributes();
                    
                    var attendanceId = hfAttendanceId.Value.AsIntegerOrNull();

                    person.NickName = tbNickName.Text;
                    person.SetBirthDate( dpBirthdate.SelectedDate );
                    person.AttributeValues["Allergy"].Value = tbAllergy.Text;
                    person.AttributeValues["Arena-16-81"].Value = tbHealthNote.Text;
                    person.AttributeValues["LegalNotes"].Value = tbLegalNote.Text;
                    


                    if( person.IsNotNull() && tbParentLocation.Text.IsNotNullOrWhiteSpace() )
                    {
                        // Update Parent Location, if changed
                        person.AttributeValues["com_northpoint.RoomCheckIn.LastParentLocation"].Value = tbParentLocation.Text;
                    }


                    //var groupId = hfGroupId.Value.AsIntegerOrNull();
                    
                    //if ( attendanceId.HasValue && groupId.HasValue )
                    //{
                    //    // If Small Group Is Selected (Not Blank)
                    //    if ( ddlSmallGroup.SelectedValueAsInt() > 0 )
                    //    {
                    //        var attendanceService = new AttendanceService( rockContext );
                    //        var attendance = attendanceService.Get( attendanceId.Value );
                    //        var groupService = new GroupService( rockContext );
                    //        var selectedGroup = groupService.Get( ddlSmallGroup.SelectedValueAsInt().Value );

                    //        // If changing to a different grade from In Room, manually update the location as well.
                    //        // If group not changed, this should return the same occurrence as the original attendance.occurrence
                    //        var selectedOccurrence = new AttendanceOccurrenceService( rockContext )
                    //            .Get( attendance.Occurrence.OccurrenceDate
                    //                     , ddlSmallGroup.SelectedValueAsInt() ?? attendance.Occurrence.GroupId
                    //                     , selectedGroup.GroupLocations.Any( gl => gl.Location == attendance.Occurrence.Location ) ? attendance.Occurrence.LocationId : selectedGroup.GroupLocations.First().LocationId  //if existing attendance location is ok for this group, use it. If not, pick the first location on the group location list.
                    //                     , attendance.Occurrence.ScheduleId );
                    //        if ( selectedOccurrence == null )
                    //        {
                    //            //create new occurrence with selected group
                    //            selectedOccurrence = new AttendanceOccurrence
                    //            {
                    //                Group = groupService.Get( ddlSmallGroup.SelectedValueAsInt().Value ) ?? null,
                    //                GroupId = ddlSmallGroup.SelectedValueAsInt(),
                    //                OccurrenceDate = attendance.Occurrence.OccurrenceDate,
                    //                LocationId = groupService.Get( ddlSmallGroup.SelectedValueAsInt().Value ).GroupLocations.First().LocationId,
                    //                ScheduleId = attendance.Occurrence.ScheduleId
                    //            };

                    //        }

                    //        if ( selectedOccurrence != attendance.Occurrence )
                    //        {
                    //            //if different occurence selected, drop out of In Room and change attendance occurrence
                    //            attendance.Occurrence = selectedOccurrence;
                    //            attendance.QualifierValueId = null;
                    //            attendance.SaveAttributeValues( rockContext );
                    //        }

                    //    }


                    //}

                    person.SaveAttributeValue( "Allergy", rockContext );
                    person.SaveAttributeValue( "Arena-16-81", rockContext );
                    person.SaveAttributeValue( "LegalNotes", rockContext );
                    person.SaveAttributeValue( "com_northpoint.RoomCheckIn.LastParentLocation", rockContext );
                    rockContext.SaveChanges();
                }
            }
        }

        protected void btnSendNote_Click( object sender, EventArgs e )
        {
            //change to a workflow
            if (tbNotes.Text.IsNotNullOrWhiteSpace())
            {
                var rockContext = new RockContext();
                var attendanceId = hfAttendanceId.Value.AsIntegerOrNull();
                var personId = hfPersonId.Value.AsIntegerOrNull();
                if (personId.HasValue)
                {
                    var personService = new PersonService( rockContext );
                    var person = personService.Get( personId.Value );

                    var workflowType = new WorkflowTypeService( rockContext ).Get( "1F660BA3-9DD0-4C25-B5D5-F1829E4A78C2".AsGuid() ); // Attendance Follow-Up Workflow (Created by Migration) TODO

                    if (workflowType.IsNotNull())
                    {
                        Dictionary<string, string> workflowAttributes = new Dictionary<string, string>();
                        workflowAttributes.Add( "AttendanceId", attendanceId.ToStringSafe() );
                        workflowAttributes.Add( "Note", tbNotes.Text.Trim() );
                        workflowType.LaunchWorkflow( "1F660BA3-9DD0-4C25-B5D5-F1829E4A78C2".AsGuid(), "Attendance Follow-Up: " + person.FullName + _startDate, workflowAttributes );
                        rockContext.SaveChanges(); //not sure if needed or not

                        nbSendNote.Text = "Note Sent to Group Director";
                        nbSendNote.Visible = true;

                        tbNotes.Text = string.Empty;
                    }
                }
                
            }
        }
        
                   

protected void dlgEdit_SaveClick( object sender, EventArgs e )
        {
            Save();

            hfAttendanceId.Value = string.Empty;
            hfPersonId.Value = string.Empty;
            hfGroupId.Value = string.Empty;
            hfGroupTypeId.Value = string.Empty;
            nbSendNote.Visible = false;

            HideDialog();
            BindEnRouteGrid();
            BindInRoomGrid();
            BindHealthNotesGrid();
            BindCheckedOutGrid();
        }

        protected void gEnRoute_CheckIn( object sender, RowEventArgs e )
        {
            var attendanceId = e.RowKeyValues["Id"] as int?;
            if ( attendanceId != null )
            {

                using ( var rockContext = new RockContext() )
                {
                    var attendanceService = new AttendanceService( rockContext );
                    var attendance = attendanceService.Get( attendanceId.Value );
                    //attendance.LoadAttributes();
                    
                    //attendance.SetAttributeValue( "com_northpoint.RoomCheckIn.InRoom", 1 );
                    //attendance.SaveAttributeValues();

                    attendance.QualifierValueId = _inRoomQualifierValueId;
                    attendance.DidAttend = true;
                    attendance.CampusId = hfCampusId.Value.AsInteger();
                    rockContext.SaveChanges();
                    
                }


                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                    if ( checkin != null )
                    {
                        checkin.InRoom = true;
                    }
                }

                BindEnRouteGrid();
            }
        }

        protected void gEnRouteOther_CheckIn( object sender, RowEventArgs e )
        {
            var attendanceId = e.RowKeyValues["Id"] as int?;
            ShowOtherLocationDialog( attendanceId );
        }


        protected void gCheckedOut_CheckIn( object sender, RowEventArgs e )
        {
            var attendanceId = e.RowKeyValues["Id"] as int?;
            if ( attendanceId != null )
            {

                using ( var rockContext = new RockContext() )
                {
                    var attendanceService = new AttendanceService( rockContext );
                    var attendance = attendanceService.Get( attendanceId.Value );
                    //attendance.LoadAttributes();

                    attendance.EndDateTime = null;

                    attendance.QualifierValueId = _inRoomQualifierValueId;
                    attendance.DidAttend = true;

                    //attendance.SetAttributeValue( "com_northpoint.RoomCheckIn.InRoom", 1 );
                    //attendance.SaveAttributeValues();

                    rockContext.SaveChanges();

                }

                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                    if ( checkin != null )
                    {
                        checkin.InRoom = true;
                        checkin.EndDateTime = null;
                        //hopefully we don't need to set the RockCache
                    }
                }

                BindCheckedOutGrid();
            }
        }

        protected void gEnRoute_CheckOut( object sender, RowEventArgs e )
        {
            var attendanceId = e.RowKeyValues["Id"] as int?;
            if ( attendanceId != null )
            {

                using ( var rockContext = new RockContext() )
                {
                    var attendanceService = new AttendanceService( rockContext );
                    var attendance = attendanceService.Get( attendanceId.Value );

                    attendance.EndDateTime = RockDateTime.Now;
                    rockContext.SaveChanges();

                    
                }

                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                    if ( checkin != null )
                    {
                        checkin.EndDateTime = RockDateTime.Now;
                        //hopefully we don't need to set the RockCache
                    }
                }
                BindEnRouteGrid();
            }
        }

        protected void gInRoom_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindInRoomGrid();
        }

        protected void gInRoom_EnRoute( object sender, RowEventArgs e )
        {
            var attendanceId = e.RowKeyValues["Id"] as int?;
            if ( attendanceId != null )
            {

                using ( var rockContext = new RockContext() )
                {
                    var attendanceService = new AttendanceService( rockContext );
                    var attendance = attendanceService.Get( attendanceId.Value );
                    //attendance.LoadAttributes();

                    //attendance.SetAttributeValue( "com_northpoint.RoomCheckIn.InRoom", 0 );
                    //attendance.SaveAttributeValues();

                    attendance.QualifierValueId = null;
                    //Do NOT remove DidAttend since they were in this room as some point

                    rockContext.SaveChanges();
                    
                }


                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                    if ( checkin != null )
                    {
                        checkin.InRoom = false;
                        //hopefully we don't need to set the RockCache
                    }
                }

                BindInRoomGrid();
            }
        }

        protected void gCheckedOut_EnRoute( object sender, RowEventArgs e )
        {
            var attendanceId = e.RowKeyValues["Id"] as int?;
            if ( attendanceId != null )
            {

                using ( var rockContext = new RockContext() )
                {
                    var attendanceService = new AttendanceService( rockContext );
                    var attendance = attendanceService.Get( attendanceId.Value );
                    //attendance.LoadAttributes();

                    attendance.EndDateTime = null;

                    //attendance.SetAttributeValue( "com_northpoint.RoomCheckIn.InRoom", 0 );
                    //attendance.SaveAttributeValues();
                    attendance.QualifierValueId = null;

                    rockContext.SaveChanges();
                    
                }
                
                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                    if ( checkin != null )
                    {
                        checkin.InRoom = false;
                        checkin.EndDateTime = null;
                        //hopefully we don't need to set the RockCache
                    }
                }

                BindCheckedOutGrid();
            }
        }

        protected void gInRoom_CheckOut( object sender, RowEventArgs e )
        {
            var attendanceId = e.RowKeyValues["Id"] as int?;
            if ( attendanceId != null )
            {
                InRoomCheckOutAttendance( attendanceId.Value );

                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                    if ( checkin != null )
                    {
                        checkin.EndDateTime = RockDateTime.Now;
                        //hopefully we don't need to set the RockCache
                    }
                }
                BindInRoomGrid();
            }
        }

        /// <summary>
        /// Checks out regular in room attendance and checks if multiple attendances 
        /// </summary>
        /// <param name="attendanceId"></param>
        private void InRoomCheckOutAttendance( int attendanceId )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );
                var attendance = attendanceService.Get( attendanceId );
                var endDateTime = RockDateTime.Now;

                attendance.EndDateTime = endDateTime;

                //Check if 'Multiple Attendances' has minutes number in block settings; if so look to see if more schedules attendances should be added
                if ( GetAttributeValue( "MinutesPassedForNewAttendance" ).AsIntegerOrNull().HasValue )
                {
                    double minutes = GetAttributeValue( "MinutesPassedForNewAttendance" ).AsDouble();
                    //if schedule end time plus minutes is less than now, add another attendance
                    if ( attendance.Occurrence.Schedule.GetCalendarEvent().DTEnd.TimeOfDay.TotalSeconds + ( minutes * 60 ) <= endDateTime.TimeOfDay.TotalSeconds )
                    {
                        var schedules = new GroupLocationService( new RockContext() ).Queryable().AsNoTracking()
                            .Where( gl => gl.GroupId == attendance.Occurrence.GroupId ).SelectMany( gl => gl.Schedules );
                        foreach ( var schedule in schedules )
                        {
                            if ( attendance.Occurrence.ScheduleId != schedule.Id && schedule.WasCheckInActive( endDateTime ) )
                            {
                                var newAttendance = attendanceService.AddOrUpdate( attendance.PersonAliasId.Value, attendance.StartDateTime, attendance.Occurrence.GroupId,
                                    attendance.Occurrence.LocationId, schedule.Id, attendance.CampusId, null, attendance.SearchTypeValueId, attendance.SearchValue,
                                    attendance.SearchResultGroupId, attendance.AttendanceCodeId, attendance.CheckedInByPersonAliasId );

                                newAttendance.EndDateTime = endDateTime;
                                newAttendance.QualifierValueId = attendance.QualifierValueId;
                                break; //Only add one attendance record
                            }
                        }
                    }
                }

                rockContext.SaveChanges();

            }
        }

        protected void gCheckedOut_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindCheckedOutGrid();
        }

        #region Dialog

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            nbSendNote.Visible = false;
            switch ( hfActiveDialog.Value )
            {
                case "EDIT":
                    dlgEdit.Show();
                    break;
                case "SMS":
                    dlgSMS.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "EDIT":
                    dlgEdit.Hide();
                    break;
                case "SMS":
                    dlgSMS.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the lbLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLogin_Click( object sender, EventArgs e )
        {
            var pinAuth = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );
            var rockContext = new Rock.Data.RockContext();
            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.GetByUserName( tbPIN.Text );
            if ( userLogin != null && userLogin.EntityTypeId.HasValue )
            {
                // make sure this is a PIN auth user login
                var userLoginEntityType = EntityTypeCache.Get( userLogin.EntityTypeId.Value );
                if ( userLoginEntityType != null && userLoginEntityType.Id == pinAuth.EntityType.Id )
                {
                    if ( pinAuth != null && pinAuth.IsActive )
                    {
                        // should always return true, but just in case
                        if ( pinAuth.Authenticate( userLogin, null ) )
                        {
                            if ( !( userLogin.IsConfirmed ?? true ) )
                            {
                                maWarning.Show( "Sorry, account needs to be confirmed.", Rock.Web.UI.Controls.ModalAlertType.Warning );
                            }
                            else if ( ( userLogin.IsLockedOut ?? false ) )
                            {
                                maWarning.Show( "Sorry, account is locked-out.", Rock.Web.UI.Controls.ModalAlertType.Warning );
                            }
                            else
                            {
                                AllowEditsSMS( true );
                                return;
                            }
                        }
                    }
                }
            }

            maWarning.Show( "Sorry, we couldn't find an account matching that PIN.", Rock.Web.UI.Controls.ModalAlertType.Warning );
        }

        private void AllowEditsSMS( bool allowEdits = true )
        {
            //Change fields on dialogs to allow Edits/SMS
            tbNickName.ReadOnly = !allowEdits;
            dpBirthdate.Enabled = allowEdits;
            tbAllergy.ReadOnly = !allowEdits;
            tbHealthNote.ReadOnly = !allowEdits;
            tbLegalNote.ReadOnly = !allowEdits;
            tbParentLocation.ReadOnly = !allowEdits;
            //ddlArea.Enabled = allowEdits;
            //ddlSmallGroup.Enabled = allowEdits;
            _textingEnabled = allowEdits && hfFromNumber.Value.IsNotNullOrWhiteSpace() && hfMessages.Value.IsNotNullOrWhiteSpace();

            //Show/Hide the PIN entry
            pnlPIN.Visible = !allowEdits;
            if ( allowEdits )
            {
                var rockContext = new RockContext();
                var person = new PersonService( rockContext ).Get( hfPersonId.ValueAsInt() );
                BindParents( rockContext, person, hfAttendanceId.ValueAsInt() );
            }
            
        }

        #endregion

        protected void timer_Tick( object sender, EventArgs e )
        {
            BindEnRouteGrid();
            BindInRoomGrid();
            BindHealthNotesGrid();
            BindCheckedOutGrid();
        }

        protected void btnPrint_Click( object sender, EventArgs e )
        {
            Save();

            try
            {
                var personId = hfPersonId.Value.AsIntegerOrNull();
                var attendanceId = hfAttendanceId.Value.AsIntegerOrNull();
                //var groupId = ddlSmallGroup.SelectedValue.AsIntegerOrNull();
                //hfGroupId.Value = groupId.ToString();

                if ( personId.HasValue && attendanceId.HasValue /* && groupId.HasValue */ )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var attendanceService = new AttendanceService( rockContext );
                        var attendance = attendanceService.Get( attendanceId.Value );
                        attendance.LoadAttributes();

                        var commonMergeFields = LavaHelper.GetCommonMergeFields( null );

                        var schedule = new CheckInSchedule
                        {
                            Schedule = new ScheduleService( rockContext ).Get(/* _scheduleId.Value*/ attendance.Occurrence.ScheduleId.Value ).Clone( false ),
                            Selected = true
                        };

                        var location = new CheckInLocation
                        {
                            Location = new LocationService( rockContext ).Get( _locationId ).Clone( false ),
                            Schedules = new List<CheckInSchedule> { schedule },
                            Selected = true
                        };

                        var group = new CheckInGroup
                        {
                            Group = new GroupService( rockContext ).Get( /*groupId*/ attendance.Occurrence.GroupId.Value ).Clone( false ),
                            Locations = new List<CheckInLocation> { location },
                            Selected = true
                        };

                        var groupType = new CheckInGroupType
                        {
                            GroupType = GroupTypeCache.Get( group.Group.GroupTypeId /*groupTypeId.Value*/ ),
                            Groups = new List<CheckInGroup> { group },
                            Labels = new List<CheckInLabel>(),
                            Selected = true
                        };

                        var person = new CheckInPerson
                        {
                            Person = new PersonService( rockContext ).Get( personId.Value ).Clone( false ),
                            SecurityCode = attendance.AttendanceCode.Code,
                            GroupTypes = new List<CheckInGroupType> { groupType },
                            FirstTime = attendanceService.Queryable().AsNoTracking().Count(a => a.PersonAlias.PersonId == personId.Value) <= 1,
                            Selected = true
                        };

                        // Only print the child's tag, never the parent tag.
                        var labelCache = GetGroupTypeLabels( groupType.GroupType ).Where( l => l.LabelType == KioskLabelType.Person || l.LabelType == KioskLabelType.Location ).OrderBy( l => l.Order ).FirstOrDefault();

                        if (labelCache != null)
                        {
                            person.SetOptions( labelCache );

                            var mergeObjects = new Dictionary<string, object>();
                            foreach ( var keyValue in commonMergeFields )
                            {
                                mergeObjects.Add( keyValue.Key, keyValue.Value );
                            }

                            mergeObjects.Add( "Location", location );
                            mergeObjects.Add( "Group", group );
                            mergeObjects.Add( "Person", person );
                            mergeObjects.Add( "GroupType", groupType );

                            
                            foreach ( var customValue in attendance.Attributes )
                            {
                                mergeObjects.Add( customValue.Key, attendance.AttributeValues[customValue.Key].Value );
                            }

                            var groupMembers = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                                .Where( m =>
                                                    m.PersonId == person.Person.Id &&
                                                    m.GroupId == group.Group.Id )
                                                .ToList();
                            mergeObjects.Add( "GroupMembers", groupMembers );

                            //get printer
                            var printer = new DeviceService( rockContext ).Get( ddlKiosk.SelectedValue.AsInteger() );

                            var label = new CheckInLabel( labelCache, mergeObjects, person.Person.Id )
                            {
                                FileGuid = labelCache.Guid,
                                PrintTo = PrintTo.Kiosk,
                                PrinterDeviceId = printer.IsNotNull() ? (int?)printer.Id : null,
                                PrinterAddress = printer.IsNotNull() ? printer.IPAddress : null
                            };
                            if ( label.PrinterDeviceId.HasValue )
                            {
                                //if ( label.PrinterDeviceId == 0 ) // print to local printer
                                //{
                                //    var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
                                //    label.PrintFrom = PrintFrom.Client;
                                //    label.PrinterAddress = "127.0.0.1";
                                //    label.LabelFile = urlRoot + label.LabelFile;
                                //    AddLabelScript( new List<CheckInLabel> { label }.ToJson() ); //copied from success.ascs.cs in checkin
                                //}
                                if ( label.PrintFrom == PrintFrom.Client )
                                {
                                    var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
                                    label.LabelFile = urlRoot + label.LabelFile;
                                    AddLabelScript( new List<CheckInLabel> { label }.ToJson() ); //copied from success.ascs.cs in checkin
                                }
                                else // print to IP address
                                {
                                    var printerDevice = new DeviceService( rockContext ).Get( label.PrinterDeviceId.Value );
                                    if ( printerDevice != null )
                                    {
                                        label.PrinterAddress = printerDevice.IPAddress;
                                        groupType.Labels.Add( label );

                                        ZebraPrint.PrintLabels( new List<CheckInLabel> { label } );
                                    }
                                }
                                
                            }

                            
                        } else
                        { btnPrint.Text = "No Label Found"; }
                    }
                }
            }
            catch ( Exception ex )
            {
                LogException( ex );
            }

        }

        // Copied from CreateLabels
        private List<KioskLabel> GetGroupTypeLabels( GroupTypeCache groupType )
        {
            var labels = new List<KioskLabel>();

            foreach ( var attribute in groupType.Attributes.OrderBy( a => a.Value.Order ) )
            {
                if ( attribute.Value.FieldType.Guid == FieldType.LABEL.AsGuid() )
                {
                    var binaryFileGuid = groupType.GetAttributeValue( attribute.Key ).AsGuidOrNull();
                    if ( binaryFileGuid != null )
                    {
                        var labelCache = KioskLabel.Get( binaryFileGuid.Value );
                        labelCache.Order = attribute.Value.Order;
                        labels.Add( labelCache );
                    }
                }
            }

            return labels;
        }

        protected void ddlKiosk_SelectedIndexChanged( object sender, EventArgs e )
        {
            Session["LocationDetail_SelectedPrinter"] = ddlKiosk.SelectedValue;
            btnPrint.Enabled = ( ddlKiosk.SelectedValue.AsIntegerOrNull() >= 0 );
        }

        protected void rAdults_OnItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item )
            {
                var parentData = ( Adult ) e.Item.DataItem;
                var rptHistory = ( Repeater ) e.Item.FindControl( "rNumbers" );

                rptHistory.DataSource = parentData.PhoneNumbersOrdered.Select(x => new {PersonAliasId = x.Person.PrimaryAliasId, x.NumberFormatted, Number = SMSNumber(x), x.NumberTypeValue});
                rptHistory.DataBind();
            }
        }

        private string SMSNumber( PhoneNumber number )
        {
            return "+" + number.CountryCode + number.Number;
        }

        protected void rNumbers_OnItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "SMS" )
            {
                HideDialog();
                var arguments = e.CommandArgument.ToString().Split( ';' );
                ShowSMSDialog( arguments[0], arguments[1] );
            }
        }

        protected void rMessages_OnItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var rockContext = new RockContext();

            // Store the current SMS settings and change them so the chosen number is the default SMS number
            var numbers = new PersonAliasService( rockContext ).Get( hfSMSAliasId.ValueAsInt() ).Person.PhoneNumbers.ToList();

            var defaultNumber = numbers.FirstOrDefault(n => n.IsMessagingEnabled);
            var defaultNumberId = -1;

            if ( defaultNumber != null )
            {
                defaultNumberId = defaultNumber.Id;
            }

            string recipientNumber = string.Empty;
            foreach ( var number in numbers )
            {
                number.IsMessagingEnabled = string.Equals( hfPhoneNumber.Value, SMSNumber( number ) );

                if ( number.IsMessagingEnabled )
                {
                    recipientNumber = number.NumberFormatted;
                }
            }

            rockContext.SaveChanges();

            // Send the SMS
            var communication = new Communication
                                {
                                    Recipients = new List<CommunicationRecipient>
                                                 {
                                                     new CommunicationRecipient
                                                     {
                                                         PersonAliasId = hfSMSAliasId.ValueAsInt()
                                                       , MediumEntityTypeId = 38
                                                     }
                                                 }
                                  , SMSFromDefinedValueId = DefinedValueCache.Get( hfFromNumber.Value ).Id
                                  , Subject = "Check-in SMS Communication"
                                  , SMSMessage = e.CommandArgument.ToString()
                                  , Status = CommunicationStatus.Approved
                                  , CommunicationType = CommunicationType.SMS
                                };
            communication.LoadAttributes();
            communication.SetAttributeValue("AttendanceId", hfAttendanceId.Value );
            communication.SetAttributeValue( "RecipientNumber", recipientNumber );
            var communicationService = new CommunicationService( rockContext );
            communicationService.Add( communication );
            rockContext.SaveChanges();
            communication.SaveAttributeValues();

            Communication.Send(communication);

            // Change the SMS settings back to what they were
            foreach ( var number in numbers )
            {
                number.IsMessagingEnabled = number.Id == defaultNumberId;
            }

            rockContext.SaveChanges();

            HideDialog();
        }

        protected void rNumbers_OnItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item )
            {
                var button = (LinkButton) e.Item.FindControl( "btnSMS" );
                var span = (HtmlGenericControl) e.Item.FindControl( "sNumber" );

                button.Visible = _textingEnabled;
                span.Visible = !_textingEnabled;

            }
        }

        private AttendanceOccurrence GetOccurrenceForLocation( RockContext rockContext, AttendanceOccurrence occurrence, int locationId, int? groupId = null, int? scheduleId = null )
        {
            var selectedOccurrence = new AttendanceOccurrenceService( rockContext ).Get( occurrence.OccurrenceDate, groupId ?? occurrence.GroupId, locationId, scheduleId ?? occurrence.ScheduleId );

            //if not selected, create it
            if ( selectedOccurrence == null )
            {
                AttendanceOccurrence newOccurrence = new AttendanceOccurrence
                {
                    ScheduleId = scheduleId ?? occurrence.ScheduleId,
                    LocationId = locationId,
                    GroupId = groupId ?? occurrence.GroupId,
                    OccurrenceDate = occurrence.OccurrenceDate
                };
                new AttendanceOccurrenceService( rockContext ).Add( newOccurrence );
                rockContext.SaveChanges();
                selectedOccurrence = new AttendanceOccurrenceService( new RockContext() ).Get( occurrence.OccurrenceDate, groupId ?? occurrence.GroupId, locationId, scheduleId ?? occurrence.ScheduleId );
            }

            return selectedOccurrence;
        }


        /// <summary>
        /// Adds the label script for client print. Copied from Success.ascx.cs from checkin
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

	    //if (navigator.userAgent.match(/(iPhone|iPod|iPad)/)) {{
     //       document.addEventListener('deviceready', onDeviceReady, false);
     //   }} else {{
     //       $( document ).ready(function() {{
     //           onDeviceReady();
     //       }});
     //   }}


	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{			
                printLabels();
            }} 
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}
		
		function alertDismissed() {{
		    // do something
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData), 
            	function(result) {{ 
			        console.log('Tag printed');
			    }},
			    function(error) {{   
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    navigator.notification.alert(
                        'An error occurred while printing the labels. ' + error[0] + ' ' + JSON.stringify(labelData),  // message plus label
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
        
        //manually start
        onDeviceReady();

", jsonObject );
            //ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
            ScriptManager.RegisterClientScriptBlock( upContent, upContent.GetType(), "addLabel", script, true );
            
        }


        protected void btnShowEdit_Click( object sender, EventArgs e )
        {
            pnlPIN.Visible = true;
            btnShowEdit.Visible = false;
        }

        protected void tbSearchOther_TextChanged( object sender, EventArgs e )
        {
            BindEnRouteGrid();
        }

        

        protected void btnCheckOut_Click( object sender, EventArgs e )
        {
            BootstrapButton button = sender as BootstrapButton;

            //Get currently Staying Ids and don't allow them to be checked out
            //List<int> stayingIds = string.IsNullOrWhiteSpace( Session["StayingIds"] as string ) ? new List<int>() : Session["StayingIds"].ToString().SplitDelimitedValues().AsIntegerList();

            //Get Schedule Id as command arg
            var scheduleId = button.CommandArgument.AsInteger();

            //Check Out each attendee with given schedule currently in the room
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );

                var checkIns = CheckIns( rockContext )
                    .Where( c => c.LocationId == _locationId )
                    .Where( c => c.InRoom == true )
                    .Where( c => c.Staying != true ) //do not remove those who are in staying mode
                    .Where( c => c.EndDateTime == null )
                    .Where( c => c.ScheduleId == scheduleId )
                    //.Where( c => !stayingIds.Contains( c.AttendanceId ) )
                    .Select( c => c.AttendanceId )
                    .ToList();

                foreach ( int attendanceId in checkIns )
                {
                    //var attendance = attendanceService.Get( attendanceId );
                    //attendance.EndDateTime = RockDateTime.Now;
                    InRoomCheckOutAttendance( attendanceId );
                }

                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    foreach ( int attendanceId in checkIns )
                    {
                        var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                        if ( checkin != null )
                        {
                            checkin.EndDateTime = RockDateTime.Now;
                        }
                    }

                    
                }

                //rockContext.SaveChanges();
            }

            BindInRoomGrid();
        }

        protected void btnEnRouteDismiss_Click( object sender, EventArgs e )
        {
            BootstrapButton button = sender as BootstrapButton;

            //Get Schedule Id as command arg
            var scheduleId = button.CommandArgument.AsInteger();

            //Check Out each attendee with given schedule currently in the room
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );

                var checkIns = CheckIns( rockContext )
                    .Where( c => c.LocationId == _locationId )
                    .Where( c => c.InRoom == false )
                    .Where( c => c.EndDateTime == null )
                    .Where( c => c.ScheduleId == scheduleId )
                    .Select( c => c.AttendanceId )
                    .ToList();

                foreach ( int attendanceId in checkIns )
                {
                    var attendance = attendanceService.Get( attendanceId );
                    attendance.EndDateTime = RockDateTime.Now;
                }

                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    foreach ( int attendanceId in checkIns )
                    {
                        var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                        if ( checkin != null )
                        {
                            checkin.EndDateTime = RockDateTime.Now;
                        }
                    }


                }

                rockContext.SaveChanges();
            }

            BindEnRouteGrid();
        }


        protected void btnStaying_Click( object sender, EventArgs e )
        {
            var button = sender as BootstrapButton;
            int? attendanceId = button.CommandArgument.AsIntegerOrNull();
            if ( attendanceId != null )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                var attendance = attendanceService.Get( attendanceId.Value );

                if ( attendance != null )
                {
                    if ( attendance.QualifierValueId != _stayingQualifierValueId )
                    {
                        attendance.QualifierValueId = _stayingQualifierValueId;
                        button.RemoveCssClass( "btn-default" );
                        button.AddCssClass( "btn-warning" );
                    }
                    else
                    {
                        attendance.QualifierValueId = _inRoomQualifierValueId;
                        button.RemoveCssClass( "btn-warning" );
                        button.AddCssClass( "btn-default" );
                    }
                }
                

                rockContext.SaveChanges();

                //manually move person in cache
                var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                if ( results != null && results.Count > 0 )
                {
                    var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                    if ( checkin != null )
                    {
                        checkin.Staying = ( attendance.QualifierValueId == _stayingQualifierValueId );
                        //hopefully we don't need to set the RockCache
                    }
                }
                

            }
            
        }

        protected void dlgOtherLocation_SaveClick( object sender, EventArgs e )
        {
            MoveOrCreateAttendance( _locationId, null, null );
        }

        /// <summary>
        /// Show Other Location checkin dialog and setup buttons
        /// </summary>
        /// <param name="attendanceId"></param>
        private void ShowOtherLocationDialog( int? attendanceId )
        {
            hfMoveAttendanceId.Value = attendanceId.ToStringSafe();

            var attendance = new AttendanceService( new RockContext() ).Get( attendanceId.Value );
            
            var groupsWithSchedules = new GroupLocationService( new RockContext() ).GetActiveByLocation( _locationId )
                .Where( gl => _groupIds.Contains( gl.GroupId ) )
                .SelectMany( gl => gl.Schedules, ( groupLocation, schedule ) => new { CreateAttendance = true, GroupId = groupLocation.GroupId, GroupName = groupLocation.Group.Name, Schedule = schedule, ScheduleId = schedule.Id, ScheduleName = schedule.Name, LocationId = groupLocation.LocationId, LocationName = groupLocation.Location.Name } ).ToList();
            foreach ( var item in groupsWithSchedules.ToList() )
            {
                //if schedule is not active now, remove that group/location/schedule option
                if ( !item.Schedule.WasScheduleOrCheckInActive( RockDateTime.Now ) )
                {
                    groupsWithSchedules.Remove( item );
                }
            }

            groupsWithSchedules.Insert( 0, new { CreateAttendance = false, GroupId = attendance.Occurrence.GroupId.Value, GroupName = attendance.Occurrence.Group.Name, attendance.Occurrence.Schedule, ScheduleId = attendance.Occurrence.ScheduleId.Value, ScheduleName = attendance.Occurrence.Schedule.Name, LocationId = _locationId, LocationName = _locationName } );
            
            dlgOtherLocation.Show();
        }

        /// <summary>
        /// Moves or creates new attendance based on selections made
        /// </summary>
        /// <param name="createAttendance"></param>
        /// <param name="locationId"></param>
        /// <param name="groupId"></param>
        /// <param name="scheduleId"></param>
        protected void MoveOrCreateAttendance( int locationId, int? groupId, int? scheduleId )
        {
            var attendanceId = hfMoveAttendanceId.ValueAsInt();
            if ( attendanceId != 0 )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                var attendance = attendanceService.Get( attendanceId );
                var nowDateTime = RockDateTime.Now;

                if ( attendance.DidAttend == true ) // if the attendance record has already been marked as InRoom before, then create a new record
                {
                    //Check Out the existing attendance record.
                    attendance.EndDateTime = nowDateTime;
                    rockContext.SaveChanges();

                    var groupLocations = new GroupLocationService( rockContext ).GetActiveByLocation( locationId );
                    //See if location has schedule for current group; if not, find any group membership with available groups
                    var selectedGroup = attendance.Occurrence.Group;
                    if ( !groupId.HasValue )
                    {
                        // If there is no group locations with current groups, then find first appropriate group
                        if ( !groupLocations.Any( gl => gl.GroupId == selectedGroup.Id ) )
                        {
                            Rock.Model.Group groupMemberGroup = null;
                            Rock.Model.Group defaultGroup = null;

                            foreach( var groupLocation in groupLocations )
                            {
                                if ( groupLocation.Group.Members.Any( m => m.PersonId == attendance.PersonAlias.PersonId ) )
                                {
                                    groupMemberGroup = groupLocation.Group;
                                }

                                if ( groupLocation.Group.GroupType.AttendanceRule == AttendanceRule.AddOnCheckIn )
                                {
                                    defaultGroup = groupLocation.Group;
                                }
                            }

                            selectedGroup = groupMemberGroup ?? defaultGroup ?? selectedGroup; //pick group member group over a default group over the existing selected group
                        }
                    }

                    //Find an active schedule
                    var selectedSchedule = attendance.Occurrence.Schedule;
                    if ( !scheduleId.HasValue )
                    {
                        // find schedules of selected group and take the first active one
                        foreach ( var schedule in groupLocations.Where( gl => gl.GroupId == selectedGroup.Id ).SelectMany( gl => gl.Schedules ) )
                        {
                            if ( schedule.WasCheckInActive( nowDateTime ) )
                            {
                                selectedSchedule = schedule;
                                break; //Only add one schedule
                            }
                        }
                    }

                    int occurrenceId = GetOccurrenceForLocation( new RockContext(), attendance.Occurrence, locationId, groupId ?? selectedGroup.Id, scheduleId ?? selectedSchedule.Id ).Id;
                    //Create brand new attendance (leave existing one alone)
                    Attendance newAttendance = new Attendance
                    {
                        PersonAliasId = attendance.PersonAliasId,
                        OccurrenceId = occurrenceId,
                        AttendanceCodeId = attendance.AttendanceCodeId,
                        CampusId = hfCampusId.Value.AsInteger(),
                        CheckedInByPersonAliasId = attendance.CheckedInByPersonAliasId,
                        CreatedByPersonAliasId = attendance.CreatedByPersonAliasId,
                        CreatedDateTime = nowDateTime,
                        DidAttend = true,
                        Note = "Created at In-Room Device: " + _locationName,
                        QualifierValueId = _inRoomQualifierValueId,
                        SearchResultGroupId = attendance.SearchResultGroupId,
                        SearchTypeValueId = attendance.SearchTypeValueId,
                        SearchValue = attendance.SearchValue,
                        StartDateTime = nowDateTime
                    };
                    rockContext = new RockContext();
                    attendanceService = new AttendanceService( rockContext );
                    attendanceService.Add( newAttendance );
                    rockContext.SaveChanges();

                    // Redo Cache (easier than adding a fake attendance object)
                    RockCache.AddOrUpdate( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, GetDataSet(), TimeSpan.FromSeconds( 20 ) );
                }
                else //attendance record has not been marked as InRoom yet; modify the existing record
                {
                    int occurrenceId = GetOccurrenceForLocation( new RockContext(), attendance.Occurrence, locationId, groupId, scheduleId ).Id;
                    //Get attenance item and change occurrence
                    rockContext = new RockContext();
                    attendanceService = new AttendanceService( rockContext );
                    attendance = attendanceService.Get( attendanceId );
                    attendance.OccurrenceId = occurrenceId;
                    attendance.QualifierValueId = _inRoomQualifierValueId;
                    attendance.DidAttend = true;
                    attendance.CampusId = hfCampusId.Value.AsInteger();
                    attendance.Note += " -InRoom Location Change (" + attendance.Occurrence.Location.Name + ")";
                    rockContext.SaveChanges();

                    //manually move person in cache
                    var results = RockCache.GetOrAddExisting( "com_northpoint.RoomCheckIn.LocationDetail." + hfCampusId.Value + "." + _checkInTypeId.ToString(), null, () => GetDataSet(), TimeSpan.FromSeconds( 20 ) ) as List<CheckInsResults>;
                    if ( results != null && results.Count > 0 )
                    {
                        var checkin = results.FirstOrDefault( r => r.AttendanceId == attendanceId );
                        if ( checkin != null )
                        {
                            checkin.InRoom = true;
                            checkin.LocationId = locationId;
                        }
                    }
                }


            }
            dlgOtherLocation.Hide();
            BindEnRouteGrid();
        }


    }


}