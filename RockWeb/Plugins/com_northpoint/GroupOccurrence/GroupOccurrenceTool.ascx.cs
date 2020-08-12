
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Barcode Roster with advanced filtering. 
/// North Point Ministries
/// </summary>
namespace RockWeb.Plugins.com_northpoint.GroupOccurrence
{
    [DisplayName( "Group Occurrence Tool" )]
    [Category( "North Point Ministries > Group Occurrence Tool" )]
    [Description( "Displays groups, using pre-defined filters, that allows group attendance occurrences to be modified or created, especially for adding a canceled occurrence" )]
    [GroupTypesField( "Group Types Include",
    "Select any specific group types to show in this block. Leave all unchecked to show all group types where 'Show in Navigation' is enabled ( except for excluded group types )",
    false, Key = "GroupTypes", Order = 2 )]
    [BooleanField( "Show School Year Filter", "Enabling this will display the school year filter for groups" )]
    [BooleanField( "Show Location Filter", "Enabling this will display the location(room) filter for groups" )]
    [BooleanField( "Show Parent Groups Filter", "Enabling this will display the parent group filter for gropus" )]
    [BooleanField( "Show Groups Filter", "Enabling this will show a groups dropdown filter" )]

    public partial class GroupOccurrenceTool : RockBlock
    {

        #region Fields

        protected Campus SelectedCampus;
        protected DateTime? startDate;
        protected DateTime? endDate;

        #endregion

        #region Base Methods



        /// <summary>
        /// OnInit
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            SetAllowedGroupTypes();


        }

        /// <summary>
        /// OnLoad
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                SetCampusByQueryStringOrCurrentPersonDefault();

                ShowHideFiltersOptions();

                dpOccurrenceDates.DelimitedValues = "Next|7|Day||";
            }

            DateRange dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( dpOccurrenceDates.DelimitedValues );
            startDate = dateRange.Start;
            endDate = dateRange.End;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// SetAllowedGroupTypes
        /// </summary>
        private void SetAllowedGroupTypes()
        {
            // limit GroupType selection to what Block Attributes allow
            hfGroupTypesInclude.Value = string.Empty;
            List<Guid> groupTypeIncludeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().AsGuidList();

            if ( groupTypeIncludeGuids.Any() )
            {
                var groupTypeIdIncludeList = new List<int>();
                foreach ( Guid guid in groupTypeIncludeGuids )
                {
                    var groupType = GroupTypeCache.Get( guid );
                    if ( groupType != null )
                    {
                        groupTypeIdIncludeList.Add( groupType.Id );
                    }
                }

                hfGroupTypesInclude.Value = groupTypeIdIncludeList.AsDelimited( "," );
            }

            hfGroupTypesExclude.Value = string.Empty;
            List<Guid> groupTypeExcludeGuids = GetAttributeValue( "GroupTypesExclude" ).SplitDelimitedValues().AsGuidList();
            if ( groupTypeExcludeGuids.Any() )
            {
                var groupTypeIdExcludeList = new List<int>();
                foreach ( Guid guid in groupTypeExcludeGuids )
                {
                    var groupType = GroupTypeCache.Get( guid );
                    if ( groupType != null )
                    {
                        groupTypeIdExcludeList.Add( groupType.Id );
                    }
                }

                hfGroupTypesExclude.Value = groupTypeIdExcludeList.AsDelimited( "," );
            }
        }

        /// <summary>
        /// SetCampusByQueryStringOrCurrentPersonDefault
        /// </summary>
        private void SetCampusByQueryStringOrCurrentPersonDefault()
        {
            CampusPicker campusPicker = FindControl( "r_CampusPicker" ) as CampusPicker;

            if ( !string.IsNullOrEmpty( Request.QueryString["campusId"] ) )
            {
                int campusId = 0;
                if ( int.TryParse( Request.QueryString["campusId"], out campusId ) )
                {
                    using ( RockContext rockContext = new RockContext() )
                    {
                        CampusService campusService = new CampusService( rockContext );
                        Campus campus = campusService.Get( campusId );
                        campusPicker.SelectedCampusId = campusId;
                        SelectedCampus = campus;
                    }
                }
            }
            else
            {
                // Set the campus filter by the person's campus
                Campus campus = CurrentPerson.GetCampus();

                if ( campus != null )
                {
                    campusPicker.SelectedCampusId = campus.Id;
                    SelectedCampus = campus;
                }
            }

            filter_Changed( campusPicker, null );
        }


        #endregion

        #region Protected Methods

        /// <summary>
        /// Block_BlockUpdated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetAllowedGroupTypes();

            SetCampusByQueryStringOrCurrentPersonDefault();

            ShowHideFiltersOptions();
        }

        /// <summary>
        /// Handle special things like hiding things or whatever.
        /// </summary>
        protected void ShowHideFiltersOptions()
        {
            dvpSchoolYear.Visible = GetAttributeValue( "ShowSchoolYearFilter" ).AsBoolean();
            rddl_ClassSelector.Visible = GetAttributeValue( "ShowLocationFilter" ).AsBoolean();
            rddl_ParentGroups.Visible = GetAttributeValue( "ShowParentGroupsFilter" ).AsBoolean();
            rddl_Groups.Visible = GetAttributeValue( "ShowGroupsFilter" ).AsBoolean();

        }



        /// <summary>
        /// Bind Grid()
        /// </summary>
        protected void BindGrid()
        {
            List<GroupItem> groupItemList = GetGroupItemList();

            Grid grid = FindControl( "grdGroups" ) as Grid;

            grid.EntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.GROUP.AsGuid() ).Id;

            grid.DataSource = groupItemList;
            grid.EntityTypeId = EntityTypeCache.Get<Group>().Id;
            grid.DataBind();

            grid.Visible = true;
        }


        /// <summary>
        /// Get the group item list
        /// </summary>
        /// <returns>List<GroupItem></returns>
        private List<GroupItem> GetGroupItemList()
        {
            List<GroupItem> groupItemList = new List<GroupItem>();

            var includedGroupTypeIds = hfGroupTypesInclude.Value.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();

            CampusPicker campusPicker = FindControl( "r_CampusPicker" ) as CampusPicker;
            RockListBox serviceTimes = FindControl( "rddl_ServiceTimeSelector" ) as RockListBox;
            RockListBox classRooms = FindControl( "rddl_ClassSelector" ) as RockListBox;
            RockListBox parentGroups = FindControl( "rddl_ParentGroups" ) as RockListBox;
            RockListBox groups = FindControl( "rddl_Groups" ) as RockListBox;

            var selectedScheduleIds = serviceTimes.SelectedValues.AsIntegerList();
            var locationIds = classRooms.SelectedValues.AsIntegerList();
            var parentGroupIds = parentGroups.SelectedValues.AsIntegerList();
            var groupIds = groups.SelectedValues.AsIntegerList();

            using ( RockContext rockContext = new RockContext() )
            {
                var groupQueryable = new GroupService( rockContext ).Queryable( "GroupType,GroupLocations,GroupLocations.Schedules" ).AsNoTracking();
                var aoQueryable = new AttendanceOccurrenceService( rockContext ).Queryable().AsNoTracking();


                // Need to only filter groups that actually takes attendance and that are active
                // and only selected group types
                groupQueryable = groupQueryable.AsNoTracking()
                    .Where(
                        p => p.GroupType.TakesAttendance == true
                        && p.IsActive == true
                        && p.IsArchived != true
                        && includedGroupTypeIds.Contains( p.GroupTypeId )
                    );

                if ( campusPicker.SelectedCampusId.HasValue )
                {
                    groupQueryable = groupQueryable.Where( p => p.CampusId == campusPicker.SelectedCampusId.Value );
                }


                // Handle the location item Picker picker
                if ( locationIds.Any() )
                {

                    groupQueryable = groupQueryable
                        .Where( p => p.GroupLocations
                              .Where( gl => locationIds.Contains( gl.Location.Id )
                          ).Any()
                    );
                }

                if ( parentGroupIds.Any() )
                {
                    groupQueryable = groupQueryable
                        .Where( p => parentGroupIds.Contains( p.ParentGroupId ?? -1 ) );
                }

                if ( groupIds.Any() )
                {
                    groupQueryable = groupQueryable
                        .Where( p => groupIds.Contains( p.Id ) );
                }

                if ( selectedScheduleIds.Any() )
                {
                    // Handle Group location schedule filters
                    groupQueryable = groupQueryable.AsQueryable().AsNoTracking()
                        .Where( p => p.GroupLocations
                              .Where( gl => gl.Schedules.AsQueryable()
                                    .Where( s => selectedScheduleIds.Contains( s.Id ) && s.IsActive == true ).Any()
                          ).Any() );
                }

                List<Group> groupList = groupQueryable.AsNoTracking().ToList();


                foreach ( Group group in groupList )
                {
                    //Check for school year, remove if group not included (continue)
                    if ( dvpSchoolYear.Visible == true && dvpSchoolYear.SelectedValueAsGuid().HasValue )
                    {
                        group.LoadAttributes();
                        if ( group.GetAttributeValue( "SchoolYear" ).IsNullOrWhiteSpace() || group.GetAttributeValue( "SchoolYear" ).AsGuidOrNull() != dvpSchoolYear.SelectedValueAsGuid() )
                        {
                            continue;
                        }
                    }

                    //Check for attendance occurrences and related attendance
                    aoQueryable = aoQueryable.Where( o => o.OccurrenceDate >= startDate && o.OccurrenceDate < endDate && o.GroupId == group.Id );
                    if ( locationIds.Any() )
                    {
                        aoQueryable = aoQueryable.Where( o => locationIds.Any( l => o.LocationId.HasValue && l == o.LocationId ) );
                    }

                    if ( selectedScheduleIds.Any() )
                    {
                        aoQueryable = aoQueryable.Where( o => selectedScheduleIds.Any( s => o.ScheduleId.HasValue && s == o.ScheduleId ) );
                    }

                    int attendanceCount = aoQueryable.SelectMany( o => o.Attendees ).Where( a => a.DidAttend.HasValue && a.DidAttend.Value ).Count();
                    int didNotOccurCount = aoQueryable.Select( o => o.DidNotOccur ).Where( b => b == true ).Count();

                    string notes = "";

                    notes += string.Format( "{0} Occurrence Records", aoQueryable.Count() );

                    if ( didNotOccurCount > 0 )
                    {
                        notes += string.Format( "<br/><b>{0} Dates marked as Did Not Occur</b>", didNotOccurCount );
                    }

                    // Find any excluded dates or just no dates in selected schedule 
                    bool anySchedulesDuringSelectedDates = false;
                    foreach ( var schedule in group.GroupLocations.SelectMany( gl => gl.Schedules ).ToList() )
                    {
                        List<DateTime> scheduleDates = schedule.GetScheduledStartTimes( startDate.Value, endDate.Value );
                        if ( scheduleDates.Any() )
                        {
                            anySchedulesDuringSelectedDates = true;
                            break;
                        }
                    }
                    if( !anySchedulesDuringSelectedDates )
                    {
                        notes += string.Format( "<br/><b>Schedule Is Excluded Or Has No Occurrences {0} - {1}</b>", startDate.Value.ToString( "MM/dd/yyyy" ), endDate.Value.ToString( "MM/dd/yyyy" ) );
                    }


                    String parentGroupName = ( group.ParentGroup != null ) ? group.ParentGroup.Name : "";
                    groupItemList.Add( new GroupItem
                    {
                        GroupId = group.Id
                            ,
                        GroupName = group.Name
                            ,
                        Campus = ( group.Campus != null ) ? group.Campus.ShortCode ?? group.Campus.Name : string.Empty
                            ,
                        ParentGroupName = parentGroupName
                            ,
                        MeetingLocation = GetGroupLocation( group.GroupLocations, locationIds )
                            ,
                        ScheduledList = GetGroupLocationSchedules( group, selectedScheduleIds, locationIds )
                            ,
                        MembersCount = group.Members.Count
                            ,
                        Attendance = attendanceCount
                            ,
                        DidNotMeet = ( didNotOccurCount > 0 )
                            ,
                        Notes = notes
                    }

                    );
                }
            }

            hfGroupIds.Value = groupItemList.Select( l => l.GroupId ).ToList().AsDelimited( "," );

            return groupItemList;
        }

        /// <summary>
        /// Creates the did not meet occurrences.
        /// </summary>
        private void CreateDidNotMeetOccurrences()
        {
            // TODO: Find all selected group location schedules and mark them as Did NOt Occur
            List<int> groupIds = hfGroupIds.Value.SplitDelimitedValues().AsIntegerList();

            RockListBox serviceTimes = FindControl( "rddl_ServiceTimeSelector" ) as RockListBox;
            RockListBox classRooms = FindControl( "rddl_ClassSelector" ) as RockListBox;
            var selectedScheduleIds = serviceTimes.SelectedValues.AsIntegerList();
            var locationIds = classRooms.SelectedValues.AsIntegerList();

            //DateTime? occurrenceDate = dpOccurrenceDate.SelectedDate;

            using ( RockContext rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );
                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                var attendanceService = new AttendanceService( rockContext );

                foreach ( int groupId in groupIds )
                {
                    var group = groupService.Get( groupId );

                    if ( group != null && startDate.HasValue && endDate.HasValue )
                    {
                        var groupLocations = group.GroupLocations.ToList();
                        if ( locationIds.Any() )
                        {
                            groupLocations = groupLocations.Where( gl => locationIds.Contains( gl.LocationId ) ).ToList();
                        }

                        foreach ( var groupLocation in groupLocations )
                        {
                            var schedules = groupLocation.Schedules.ToList();
                            if ( selectedScheduleIds.Any() )
                            {
                                schedules = schedules.Where( s => selectedScheduleIds.Contains( s.Id ) ).ToList();
                            }

                            // Add or Get Occurrence
                            foreach ( var schedule in schedules )
                            {
                                List<DateTime> scheduleDates = schedule.GetScheduledStartTimes( startDate.Value, endDate.Value );

                                foreach ( var date in scheduleDates )
                                {
                                    var occurrence = attendanceOccurrenceService.Get( date, group.Id, groupLocation.LocationId, schedule.Id );
                                    if ( occurrence == null )
                                    {
                                        // If occurrence does not yet exist, create it
                                        // A new context is used so the occurrence can be saved and used on multiple new attendance records that will be saved at once.
                                        using ( var newContext = new RockContext() )
                                        {
                                            occurrence = new AttendanceOccurrence
                                            {
                                                OccurrenceDate = date,
                                                GroupId = group.Id,
                                                LocationId = groupLocation.LocationId,
                                                ScheduleId = schedule.Id,
                                            };

                                            var newOccurrenceService = new AttendanceOccurrenceService( newContext );
                                            newOccurrenceService.Add( occurrence );
                                            newContext.SaveChanges();

                                            // Query for the new occurrence using original context.
                                            occurrence = attendanceOccurrenceService.Get( occurrence.Id );
                                        }
                                    }

                                    // Mark is Did Not Meet
                                    occurrence.DidNotOccur = true;

                                    foreach ( var attendance in occurrence.Attendees )
                                    {
                                        attendance.DidAttend = false;
                                    }
                                }


                                rockContext.SaveChanges();
                            }
                        }
                    }
                }

            }




        }


        /// <summary>
        /// Builds the locations and schedules
        /// </summary>
        /// <param name="group">Group</param>
        /// <returns>String</returns>
        private string GetGroupLocationSchedules( Group group, List<int> scheduleIds, List<int> locationIds )
        {
            string str = string.Empty;
            List<string> groupSchedules = new List<string>();

            if ( group.Schedule != null && scheduleIds.Contains( group.Schedule.Id ) )
            {
                groupSchedules.Add( group.Schedule.Name );
            }

            if ( group.GroupLocations.Any() )
            {
                foreach ( GroupLocation groupLocation in group.GroupLocations.Where( gl => !locationIds.Any() || locationIds.Contains( gl.LocationId ) ) )
                {
                    string name = ( groupLocation.Location.IsNamedLocation ) ? groupLocation.Location.Name : groupLocation.Location.FormattedAddress;

                    if ( groupLocation.Schedules.Any() )
                    {
                        string schedules = string.Join( "; ", groupLocation.Schedules.Where( s => scheduleIds.Contains( s.Id ) )
                            .Select( gls => gls.Name ).ToArray() );
                        groupSchedules.Add( string.Format( "{0} @ {1}", name, schedules ) );

                    }
                    else
                    {
                        groupSchedules.Add( string.Format( "{0} (no set schedule)", name ) );
                    }
                }
            }

            str = string.Join( ", ", groupSchedules.ToArray() );

            return str;

        }

        /// <summary>
        /// Get the group location
        /// </summary>
        /// <param name="groupLocations">ICollection<GroupLocation></param>
        /// <returns>string</returns>
        private string GetGroupLocation( ICollection<GroupLocation> groupLocations, List<int> locationIds )
        {
            string s = "";

            if ( groupLocations.Any() )
            {
                List<string> namedLocationNames = groupLocations.Where( gl => gl.Location.IsNamedLocation == true
                         && ( locationIds.Contains( gl.LocationId ) || !locationIds.Any() ) )
                    .Select( p => p.Location.Name )
                    .ToList();

                List<string> addressLocations = groupLocations.Where( gl => gl.Location.IsNamedLocation == false
                         && ( locationIds.Contains( gl.LocationId ) || !locationIds.Any() ) )
                    .Select( p => p.Location.FormattedAddress )
                    .ToList();

                string[] list;

                if ( namedLocationNames.Count > 0 )
                {
                    list = namedLocationNames.Concat( addressLocations ).ToArray();
                }
                else
                {
                    list = addressLocations.ToArray();
                }

                s = string.Join( ", ", list );

            }

            return s;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles every filter changed event by getting the ID of the changed object and cascading down changes. Campus > Service Time > School Year > Location > Parent Group > Group
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void filter_Changed( object sender, EventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                //Get ID of control that was changed
                string id = ( ( Control ) sender ).ID;

                bool continueNext = false;

                var includedGroupTypeIds = hfGroupTypesInclude.Value.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();

                // Campus Picker
                if ( continueNext || id == "r_CampusPicker" )
                {
                    List<int> alreadySelectedServiceTimes = rddl_ServiceTimeSelector.SelectedValuesAsInt;

                    List<ListItem> listItems = new List<ListItem>();
                    if ( r_CampusPicker.SelectedCampusId.HasValue )
                    {

                        var campusId = CampusCache.Get( r_CampusPicker.SelectedCampusId.Value ).Id;
                        var groupService = new GroupService( rockContext );
                        List<Schedule> schedules = groupService.Queryable().AsNoTracking()
                            .Where( g => g.IsActive == true && g.IsArchived != true )
                            .Where( g => g.CampusId == campusId )
                            .Where( g => includedGroupTypeIds.Contains( g.GroupTypeId ) )
                            .SelectMany( g => g.GroupLocations.SelectMany( gl => gl.Schedules ).Where( s => s.IsActive == true ) )
                            .Distinct().ToList();

                        if ( schedules.Any() )
                        {
                            //TODO Fix Ordering
                            //schedules = schedules.OrderBy(s => s.GetCalendarEvent().DTStart.Hour).ThenBy(s => s.GetCalendarEvent().DTStart.Minute).ToList();
                            schedules = schedules.OrderBy( s => s.WeeklyDayOfWeek ).ThenBy( s => s.WeeklyTimeOfDay )
                                .ThenBy( s => ( s.GetCalendarEvent() != null ? s.GetCalendarEvent().DTStart.Hour : 24 ) )
                                .ThenBy( s => ( s.GetCalendarEvent() != null ? s.GetCalendarEvent().DTStart.Minute : 59 ) )
                                .ToList();
                            foreach ( Schedule schedule in schedules )
                            {
                                listItems.Add( new ListItem()
                                {
                                    Text = schedule.Name,
                                    Value = schedule.Id.ToString(),
                                    Selected = ( alreadySelectedServiceTimes.Contains( schedule.Id ) ? true : false )
                                } );
                            }
                        }

                    }


                    // Populate the control
                    rddl_ServiceTimeSelector.DataSource = listItems;
                    rddl_ServiceTimeSelector.DataBind();

                    // Need to manually select items
                    foreach ( ListItem item in listItems.Where( i => i.Selected == true ) )
                    {
                        var foundItem = rddl_ServiceTimeSelector.Items.FindByValue( item.Value );
                        if ( foundItem.IsNotNull() )
                        {
                            foundItem.Selected = true;
                        }
                    }


                    // continue through next items
                    continueNext = true;
                }

                // Schedule (Service Time)
                if ( continueNext || id == "rddl_ServiceTimeSelector" )
                {
                    if ( dvpSchoolYear.Visible )
                    {
                        Guid? alreadySelectedYear = dvpSchoolYear.SelectedValueAsGuid();

                        // Reset School Year Selections
                        List<ListItem> yearListItems = new List<ListItem>();
                        var schoolYearDV = new DefinedValueService( rockContext ).GetByDefinedTypeId( 189 );
                        if ( schoolYearDV.IsNotNull() && schoolYearDV.Any() )
                        {
                            var schoolYearsList = schoolYearDV.ToList().Reverse<DefinedValue>();
                            foreach ( var year in schoolYearsList )
                            {
                                yearListItems.Add( new ListItem()
                                {
                                    Text = year.Value,
                                    Value = year.Guid.ToString(),
                                    Selected = ( year.Guid == alreadySelectedYear ? true : false )
                                } );
                            }

                            dvpSchoolYear.DataSource = yearListItems;
                            dvpSchoolYear.DataBind();

                            // Need to manually select items
                            foreach ( ListItem item in yearListItems.Where( i => i.Selected == true ) )
                            {
                                var foundItem = dvpSchoolYear.Items.FindByValue( item.Value );
                                if ( foundItem.IsNotNull() )
                                {
                                    foundItem.Selected = true;
                                }
                            }
                        }
                        else
                        {
                            dvpSchoolYear.Visible = false;
                        }
                    }


                    // continue through next items
                    continueNext = true;
                }

                // School Year 
                if ( continueNext || id == "dvpSchoolYear" )
                {
                    var alreadySelectedLocations = rddl_ClassSelector.SelectedValuesAsInt;

                    List<ListItem> listItems = new List<ListItem>();
                    // Set up Locations (even if not displaying )
                    if ( rddl_ServiceTimeSelector.SelectedValues.Any()
                        && r_CampusPicker.SelectedCampusId.HasValue && hfGroupTypesInclude.Value.IsNotNullOrWhiteSpace() )
                    {

                        var campusId = CampusCache.Get( r_CampusPicker.SelectedCampusId.Value ).Id;
                        var scheduleIds = rddl_ServiceTimeSelector.SelectedValuesAsInt;

                        var groupService = new GroupService( rockContext );

                        List<Location> locations = groupService.Queryable( "GroupLocations,GroupLocations.Schedules" ).AsNoTracking()
                            .Where( g => g.IsActive == true && g.IsArchived != true )
                            .Where( g => g.CampusId == campusId )
                            .Where( g => includedGroupTypeIds.Contains( g.GroupTypeId ) )
                            .SelectMany( g => g.GroupLocations.Where( gl => gl.Schedules.Any( s => scheduleIds.Contains( s.Id ) && s.IsActive == true ) ) )
                            .Select( gl => gl.Location ).DistinctBy( l => l.Id )
                            .OrderBy( l => l.Name )
                            .ToList();


                        //Add a 'Select All' Functionality to the list
                        ListItem item = new ListItem
                        {
                            Text = "Select All",
                            Value = "0"
                        };

                        listItems.Add( item );

                        foreach ( Location location in locations )
                        {
                            listItems.Add( new ListItem
                            {
                                Text = location.Name,
                                Value = location.Id.ToString(),
                                Selected = ( ( alreadySelectedLocations.Contains( 0 ) || alreadySelectedLocations.Contains( location.Id ) ) ? true : false )
                            } );
                        }


                    }
                    rddl_ClassSelector.DataSource = listItems;
                    rddl_ClassSelector.DataBind();

                    // Need to manually select items
                    foreach ( ListItem item in listItems.Where( i => i.Selected == true ) )
                    {
                        var foundItem = rddl_ClassSelector.Items.FindByValue( item.Value );
                        if ( foundItem.IsNotNull() )
                        {
                            foundItem.Selected = true;
                        }
                    }

                    // continue through next items
                    continueNext = true;
                }

                // Location (Classroom)
                if ( continueNext || id == "rddl_ClassSelector" )
                {
                    if ( rddl_ParentGroups.Visible && rddl_ServiceTimeSelector.SelectedValues.Any() && r_CampusPicker.SelectedCampusId.HasValue )
                    {
                        List<int> alreadySelectedParents = rddl_ParentGroups.SelectedValuesAsInt;
                        var queryable = new GroupService( rockContext ).Queryable( "GroupType,GroupLocations,GroupLocations.Schedules" ).AsNoTracking();

                        queryable = queryable.AsNoTracking()
                            .Where(
                                p => p.GroupType.TakesAttendance == true
                                && p.IsActive == true
                                && p.IsArchived != true
                                && includedGroupTypeIds.Contains( p.GroupTypeId )
                            );

                        if ( r_CampusPicker.SelectedCampusId.HasValue )
                        {
                            queryable = queryable.Where( p => p.CampusId == r_CampusPicker.SelectedCampusId.Value );
                        }

                        if ( rddl_ServiceTimeSelector.SelectedValues.Any() )
                        {
                            var selectedScheduleIds = rddl_ServiceTimeSelector.SelectedValuesAsInt;
                            queryable = queryable.Where( p => p.GroupLocations
                              .Where( gl => gl.Schedules.AsQueryable()
                                    .Where( s => selectedScheduleIds.Contains( s.Id ) && s.IsActive == true ).Any()
                                 ).Any() );
                        }

                        // Handle the location item Picker picker
                        if ( rddl_ClassSelector.SelectedValues.Any() )
                        {
                            var locationids = rddl_ClassSelector.SelectedValuesAsInt;
                            queryable = queryable
                                .Where( p => p.GroupLocations
                                      .Where( gl => locationids.Contains( gl.Location.Id )
                                  ).Any()
                            );
                        }


                        //Add a 'Select All' Functionality to the list
                        ListItem item = new ListItem
                        {
                            Text = "Select All",
                            Value = "0"
                        };

                        List<ListItem> listItems = new List<ListItem>();
                        listItems.Add( item );

                        foreach ( Group parentGroup in queryable.Where( g => g.ParentGroupId.HasValue ).Select( g => g.ParentGroup ).DistinctBy( g => g.Id ).OrderBy( g => g.Name ).ToList() )
                        {
                            ListItem listItem = new ListItem
                            {
                                Text = parentGroup.Name,
                                Value = parentGroup.Id.ToString(),
                                Selected = ( ( alreadySelectedParents.Contains( 0 ) || alreadySelectedParents.Contains( parentGroup.Id ) ) ? true : false )
                            };


                            if ( !listItems.Contains( listItem ) )
                            {
                                listItems.Add( listItem );
                            }
                        }

                        rddl_ParentGroups.DataSource = listItems;
                        rddl_ParentGroups.DataBind();

                        // Need to manually select items
                        foreach ( ListItem itemSelected in listItems.Where( i => i.Selected == true ) )
                        {
                            var foundItem = rddl_ParentGroups.Items.FindByValue( itemSelected.Value );
                            if ( foundItem.IsNotNull() )
                            {
                                foundItem.Selected = true;
                            }
                        }

                    }

                    // continue through next items
                    continueNext = true;
                }

                // Parent Group Picker
                if ( continueNext || id == "rddl_ParentGroups" )
                {

                    if ( rddl_Groups.Visible && rddl_ServiceTimeSelector.SelectedValues.Any() && r_CampusPicker.SelectedCampusId.HasValue )
                    {
                        List<int> alreadySelectedGroups = rddl_Groups.SelectedValuesAsInt;
                        var queryable = new GroupService( rockContext ).Queryable( "GroupType,GroupLocations,GroupLocations.Schedules" ).AsNoTracking();

                        queryable = queryable.AsNoTracking()
                            .Where(
                                p => p.GroupType.TakesAttendance == true
                                && p.IsActive == true
                                && p.IsArchived != true
                                && includedGroupTypeIds.Contains( p.GroupTypeId )
                            );

                        if ( r_CampusPicker.SelectedCampusId.HasValue )
                        {
                            queryable = queryable.Where( p => p.CampusId == r_CampusPicker.SelectedCampusId.Value );
                        }

                        if ( rddl_ServiceTimeSelector.SelectedValues.Any() )
                        {
                            var selectedScheduleIds = rddl_ServiceTimeSelector.SelectedValuesAsInt;
                            queryable = queryable.Where( p => p.GroupLocations
                              .Where( gl => gl.Schedules.AsQueryable()
                                    .Where( s => selectedScheduleIds.Contains( s.Id ) && s.IsActive == true ).Any()
                                 ).Any() );
                        }

                        // Handle the location item Picker picker
                        if ( rddl_ClassSelector.SelectedValues.Any() )
                        {
                            var locationids = rddl_ClassSelector.SelectedValuesAsInt;
                            queryable = queryable
                                .Where( p => p.GroupLocations
                                      .Where( gl => locationids.Contains( gl.Location.Id )
                                  ).Any()
                            );
                        }

                        //Handle parent groups
                        if ( rddl_ParentGroups.SelectedValues.Any() )
                        {
                            var parentGroupIds = rddl_ParentGroups.SelectedValuesAsInt;
                            queryable = queryable
                                .Where( p => parentGroupIds.Contains( p.ParentGroupId ?? -1 ) );
                        }


                        //Add a 'Select All' Functionality to the list
                        ListItem item = new ListItem
                        {
                            Text = "Select All",
                            Value = "0"
                        };

                        List<ListItem> listItems = new List<ListItem>();
                        listItems.Add( item );

                        foreach ( Group group in queryable.DistinctBy( g => g.Id ).OrderBy( g => g.Name ).ToList() )
                        {
                            ListItem listItem = new ListItem
                            {
                                Text = group.Name,
                                Value = group.Id.ToString(),
                                Selected = ( ( alreadySelectedGroups.Contains( 0 ) || alreadySelectedGroups.Contains( group.Id ) ) ? true : false )
                            };


                            if ( !listItems.Contains( listItem ) )
                            {
                                listItems.Add( listItem );
                            }
                        }

                        rddl_Groups.DataSource = listItems;
                        rddl_Groups.DataBind();

                        // Need to manually select items
                        foreach ( ListItem itemSelected in listItems.Where( i => i.Selected == true ) )
                        {
                            var foundItem = rddl_Groups.Items.FindByValue( itemSelected.Value );
                            if ( foundItem.IsNotNull() )
                            {
                                foundItem.Selected = true;
                            }
                        }

                    }
                    // continue through next items
                    continueNext = true;
                }

                // Groups Picker
                if ( continueNext || id == "rddl_Groups" )
                {

                    // continue through next items
                    continueNext = true;
                }

            }

        }


        /// <summary>
        /// rbb_Filter_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rbb_Filter_Click( object sender, EventArgs e )
        {
            BindGrid();
        }


        // called by page
        protected void refreshUrl_Click( object sender, EventArgs e )
        {
            BindGrid();
        }


        #endregion

        #region Classes

        /// <summary>
        /// GroupItem
        /// </summary>
        public class GroupItem : RockDynamic
        {
            public int GroupId { get; set; }

            public string GroupName { get; set; }

            public string Campus { get; set; }

            public string ParentGroupName { get; set; }

            public int MembersCount { get; set; }

            public string ScheduledList { get; set; }

            public string MeetingLocation { get; set; }

            public int Attendance { get; set; }

            public bool DidNotMeet { get; set; }

            public string Notes { get; set; }

        }


        #endregion


        protected void r_CampusPicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            filter_Changed( sender, e );
        }
        protected void rddl_ServiceTimeSelector_SelectedIndexChanged( object sender, EventArgs e )
        {
            filter_Changed( sender, e );
        }

        protected void dvpSchoolYear_SelectedIndexChanged( object sender, EventArgs e )
        {
            filter_Changed( sender, e );
        }

        protected void rddl_ClassSelector_SelectedIndexChanged( object sender, EventArgs e )
        {
            filter_Changed( sender, e );
        }

        protected void rddl_ParentGroups_SelectedIndexChanged( object sender, EventArgs e )
        {
            filter_Changed( sender, e );
        }

        protected void rddl_Groups_SelectedIndexChanged( object sender, EventArgs e )
        {
            filter_Changed( sender, e );
        }


        protected void btnMarkDidNotMeet_Click( object sender, EventArgs e )
        {
            CreateDidNotMeetOccurrences();
            BindGrid();
        }
    }
}