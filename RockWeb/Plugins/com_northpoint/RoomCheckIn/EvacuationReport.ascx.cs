using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    /// <summary>
    /// Displays the calendars that user is authorized to view.
    /// </summary>
    [DisplayName( "Evacuation Report" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Displays all the attendance by room" )]
    [CodeEditorField( "Header Lava Template", "Use this field to construct HTML Table for Header of Evac Report ( Lava Merge Fields Avail: {{CheckInResults}} )", CodeEditorMode.Lava, CodeEditorTheme.Rock, order: 1)]
    [CodeEditorField( "Body Lava Template", "Use this field to construct HTML Table for body of the Evac Report ( Lava Merge Fields Avail: {{CheckInResults}} )", CodeEditorMode.Lava, CodeEditorTheme.Rock, order: 2)]

    public partial class EvacuationReport : Rock.Web.UI.RockBlock
    {
        private int checkInTypeId;
        private int campusId;
        private int? locationId;
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            checkInTypeId = PageParameter( "CheckinTypeId" ).AsInteger();
            campusId = PageParameter( "CampusId" ).AsInteger();
            locationId = PageParameter( "LocationId" ).AsIntegerOrNull();

            if (!IsPostBack)
            {
                DisplayReport();
            }
        }

        private void DisplayReport()
        {
            RockContext rockContext = new RockContext();
            DateTime tomorrow = RockDateTime.Today.AddDays( 1 );

            //load groupLocations
            List<int> locationIds = new List<int>();
            
            var groupTypes = new GroupTypeService( rockContext ).GetChildGroupTypes( checkInTypeId ).Select( t => t.Id );
            var groups = new GroupService( rockContext ).Queryable( "GroupLocations" )
                .Where( g => groupTypes.Contains( g.GroupTypeId ) && g.IsActive );
            var _groupLocationIds = groups
                .SelectMany( g => g.GroupLocations ).ToList().Where( gl => gl.Location.CampusId == campusId && gl.LocationId == (locationId ?? gl.LocationId ) ).Select( gl => gl.Id ).ToList();


            locationIds = new GroupLocationService( new RockContext() ).Queryable().Where( gl => _groupLocationIds.Contains( gl.Id ) ).Select( gl => gl.LocationId ).ToList();

            var attendanceQry = new AttendanceService( new RockContext() ).Queryable()
                .Where( a => locationIds.Contains( a.Occurrence.LocationId ?? -1 ) )
                .Where( a => a.StartDateTime >= RockDateTime.Today && a.StartDateTime < tomorrow );

            //Load attributes
            //attendanceQry.LoadAttributes();

            //select new objects with sums and counts, grouped by Occurrence (location+schedule)
            var dataSource = attendanceQry
                .GroupBy( a => a.Occurrence.LocationId ).ToList()
                .Select( t => new
                {
                    //may need to add aggregate to show multiple parent group names
                    Area = t.First().Occurrence.Group.ParentGroup.Name,
                    AreaOrder = t.First().Occurrence.Group.ParentGroup.Order,
                    LocationId = t.Key ?? -1,
                    Location = t.First().Occurrence.Location.Name,
                    EnRoute = t.Sum( t1 => ( t1.QualifierValueId == null && t1.EndDateTime == null ) ? 1 : 0 ),
                    InRoom = t.Sum( t2 => ( t2.QualifierValueId != null && t2.EndDateTime == null ) ? 1 : 0 ),
                    CheckedOut = t.Sum( t3 => ( t3.EndDateTime != null ) ? 1 : 0 ),
                    Total = t.Count(),
                    //Add List of Attendances for By-Room Pages
                    Attendances = t.OrderBy( p => p.PersonAlias.Person.NickName ?? p.PersonAlias.Person.FirstName ).ToList()
                } ).ToList();

            var emptyItems = new GroupLocationService( new RockContext() ).GetByIds( _groupLocationIds )
                .GroupBy( gl => gl.LocationId ).ToList()
                .Select( t => new
                {
                    Area = t.First().Group.ParentGroup.Name,
                    AreaOrder = t.First().Group.ParentGroup.Order,
                    LocationId = t.Key,
                    Location = t.First().Location.Name,
                    EnRoute = 0,
                    InRoom = 0,
                    CheckedOut = 0,
                    Total = 0,
                    Attendances = new List<Attendance>()
                } ).ToList();

            //if dataSource doesn't have a location in the emptyItems list, add it
            foreach ( var item in emptyItems )
            {
                // if locationId not found, add the blank one
                if ( !dataSource.Any( i => i.LocationId == item.LocationId ) )
                {
                    dataSource.Add( item );
                }
            }

            //Order by Location Alpha Name
            dataSource = dataSource.OrderBy( a => a.Location ).ToList();
            

            //Create HTML
            string html = "";

            var config = new GroupTypeService( new RockContext() ).Get( checkInTypeId );
            config.LoadAttributes(); 
            
            
            //Merge lava and add to html string
            var header = GetAttributeValue( "HeaderLavaTemplate" );
            var commonMergeFields = LavaHelper.GetCommonMergeFields( null );
            var mergeObjects = new Dictionary<string, object>();
            foreach ( var keyValue in commonMergeFields )
            {
                mergeObjects.Add( keyValue.Key, keyValue.Value );
            }

            //Add in {{CheckInResults}} from datasource for lava merge
            mergeObjects.Add( "CheckInResults", dataSource );
            

            //Add in {{Campus}}
            mergeObjects.Add( "Campus", CampusCache.Get( campusId ) );

            //Add in {{CheckInType}}
            mergeObjects.Add( "CheckInType", config );

            //Merge Header Lava
            html = header.ResolveMergeFields( mergeObjects );
            
            //Get Body Lava
            var body = GetAttributeValue( "BodyLavaTemplate" );
            
            //Merge Body Lava
            html += body.ResolveMergeFields( mergeObjects );
            
            lHtml.Text = html;
        }

        
        
    }
}