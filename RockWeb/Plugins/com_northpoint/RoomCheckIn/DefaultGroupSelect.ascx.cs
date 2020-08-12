using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName("Default Group Select")]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Displays default group messages and confirmation" )]

    [BooleanField( "Bypass Text Screens", "If yes, the text exceptions are not displayed and next page is displayed after processing", defaultValue: false, IsRequired = true )]
    [CodeEditorField( "No Regular or Default Groups Found", "HTML to display when there are no groups (including default groups) found active for selected. {{Person}}, {{Kiosk}}", mode: Rock.Web.UI.Controls.CodeEditorMode.Lava )]
    [CodeEditorField( "No Groups At Current Location", "HTML to display when there are no groups found at current location for selected. {{Person}}, {{Kiosk}}", mode: Rock.Web.UI.Controls.CodeEditorMode.Lava )]
    [CodeEditorField( "No Groups At Current Schedule", "HTML to display when there are no groups found at current schedule for selected. {{Person}}, {{Kiosk}}", mode: Rock.Web.UI.Controls.CodeEditorMode.Lava )]
    [CodeEditorField( "No Regular Groups Found", "HTML to display when there are no groups in group type found active for selection. {{Person}}, {{Kiosk}}", mode: Rock.Web.UI.Controls.CodeEditorMode.Lava )]
    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true, IsRequired = true )]
    [BooleanField( "Query", "Select 'Yes' to query database why no regular groups available. If false, only 'No Regular Groups Found' text will be displayed", false, IsRequired = true )]
    [BooleanField( "Load Balance", "Select 'Yes' to load balance groups with the same Parent Group", false, IsRequired = true )]
    ///
    /// Block to deactivate default groups, if regular groups are available. Show messages appropriate to situation and provide a confirmation 'next' button.
    /// These rulings are based on being in the same group types
    ///
    public partial class DefaultGroupSelect : CheckInBlock
    {
        //property to get list of persons
        private List<int> processedPersons
        {
            get
            {
                if( ViewState["processedPersons"] == null )
                {
                    ViewState["processedPersons"] = new List<int>();
                }
                return ViewState["processedPersons"] as List<int>;
            }
            set
            {
                ViewState["processedPersons"] = value;
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-familyselect-bg" );
            }

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    
                    ProcessNextPersonDefaultGroups();
                    
                }
            }
        }

        private void ProcessNextPersonDefaultGroups( )
        {
            // var person = CurrentCheckInState.CheckIn.CurrentPerson;  //Recycle block for each selected person in family
            CheckInPerson person = null;
            try
            {
                person = CurrentCheckInState.CheckIn.CurrentFamily.People.FirstOrDefault( p => p.PreSelected == true && !processedPersons.Contains( p.Person.Id ) );
            }
            catch
            {
                NavigateToNextPage();
                return;
            }

            if( person == null )
            {
                NavigateToNextPage();
                return;
            }
            else
            {
                processedPersons.Add( person.Person.Id );
            }

            var remove = GetAttributeValue( "Remove" ).AsBoolean();

            //If no groups at all, show 'No Regular or Default Groups Found'
            if ( !person.GroupTypes.SelectMany( t => t.Groups ).Any() )
            {
                DisplayBodyMessage( GetAttributeValue( "NoRegularorDefaultGroupsFound" ) ?? "", person.Person );
            }

            
            //If any groups available for check-in that are not default groups, use those; exclude or remove the default groups
            else if ( person.GroupTypes.SelectMany( t => t.Groups.Select( g => g.Group ) ).Any( g => ( g.GetAttributeValue( "com_northpoint.RoomCheckIn.IsDefault" ).AsBooleanOrNull() ?? g.GetAttributeValue( "IsDefault" ).AsBooleanOrNull() ?? false ) == false ) )
            {
                //for each group type, remove groups that are default
                foreach ( CheckInGroupType groupType in person.GroupTypes )
                {
                    var groupsToRemove = groupType.Groups.Where( g => ( g.Group.GetAttributeValue( "com_northpoint.RoomCheckIn.IsDefault" ).AsBooleanOrNull() ?? g.Group.GetAttributeValue( "IsDefault" ).AsBooleanOrNull() ?? false ) == true ).ToList();
                    if( remove )
                    {
                        groupType.Groups.RemoveAll( g => groupsToRemove.Contains( g ) );
                    }
                    else
                    {
                        foreach( CheckInGroup group in groupsToRemove )
                        {
                            group.ExcludedByFilter = true;
                        }
                    }
                }
                //Remove empty group types
                if ( remove )
                {
                    person.GroupTypes.RemoveAll( gt => !gt.Groups.Any() );
                }
                else
                {
                    var groupTypesToExclude = person.GroupTypes.Where( gt => gt.Groups.All( g => g.ExcludedByFilter == true ) ).ToList();
                    foreach( CheckInGroupType groupType in groupTypesToExclude )
                    {
                        groupType.ExcludedByFilter = true;
                    }
                }
                
                
                SaveState();
                //NavigateToNextPage();
                ProcessNextPersonDefaultGroups(); //will go to next page after no more people.

            }
            else //there were no regular groups, only default. Do some querying to see why. (Bypass text screens if selected)
            {
                //Remove default groups based on grade or lack thereof
                RemoveDefaultGroupsByGrade( person );

                if ( GetAttributeValue( "LoadBalance" ).AsBoolean() )
                {
                    LoadBalanceByParentGroup( person );
                }

                if ( !GetAttributeValue( "BypassTextScreens" ).AsBoolean() )
                {
                    if ( GetAttributeValue( "Query" ).AsBooleanOrNull() ?? true )
                    {
                        var groupMemberService = new GroupMemberService( new Rock.Data.RockContext() ).Queryable( "Group,Group.GroupLocations,Group.GroupLocations.Schedules" ).AsNoTracking();
                        List<int> kioskGroupTypeIds = new List<int>();
                        foreach ( var kioskGroupType in CurrentCheckInState.Kiosk.KioskGroupTypes )
                        {
                            kioskGroupTypeIds.Add( kioskGroupType.GroupType.Id );
                            kioskGroupTypeIds.AddRange( new GroupTypeService( new Rock.Data.RockContext() ).GetAllAssociatedDescendents( kioskGroupType.GroupType.Id ).Select( gt => gt.Id ) );
                        }

                        var groups = groupMemberService.Where( gm => gm.PersonId == person.Person.Id )
                            .Where( gm => gm.Group.IsActive == true && gm.Group.IsArchived != true )
                            .Where( gm => CurrentCheckInState.ConfiguredGroupTypes.Contains( gm.Group.GroupTypeId ) )
                            .Where( gm => kioskGroupTypeIds.Contains( gm.Group.GroupTypeId ) )
                            .Select( gm => gm.Group ).ToList();

                        //See if locations of groups are not at current campus
                        if ( !groups.Any() )
                        {
                            DisplayBodyMessage( GetAttributeValue( "NoRegularGroupsFound" ) ?? "", person.Person );
                        }
                        else if ( !groups.Any( g => g.GroupLocations.Any( gl => gl.Location.CampusId.HasValue && CurrentCheckInState.Kiosk.Device.Locations.Any( l => l.CampusId == gl.Location.CampusId ) ) ) )
                        {
                            DisplayBodyMessage( GetAttributeValue( "NoGroupsAtCurrentLocation" ) ?? "", person.Person );
                        }
                        //See if group schedules weren't active
                        else if ( !groups.Any( g => g.GroupLocations.Any( gl => gl.Schedules.Any( s => s.WasCheckInActive( RockDateTime.Now ) ) ) ) )
                        {
                            DisplayBodyMessage( GetAttributeValue( "NoGroupsAtCurrentSchedule" ) ?? "", person.Person );
                        }
                        else
                        {
                            DisplayBodyMessage( "~" + GetAttributeValue( "NoRegularGroupsFound" ) ?? "", person.Person );
                        }
                    }
                    else
                    {
                        //Catch all
                        DisplayBodyMessage( GetAttributeValue( "NoRegularGroupsFound" ) ?? "", person.Person );
                    }
                }
                else
                {
                    //Bypass text screens and move to next person
                    ProcessNextPersonDefaultGroups();
                }
                

            }
        }

        private void DisplayBodyMessage( string message, Person person )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, person, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            
            mergeFields.Add( "Kiosk", CurrentCheckInState.Kiosk );
            mergeFields.Add( "SelectedFamily", CurrentCheckInState.CheckIn.CurrentFamily.People.Where( p => p.PreSelected == true && !processedPersons.Contains( p.Person.Id ) ).Select( p => p.Person.FullName ).ToList().AsDelimited(",") );
            

            var lavaTemplate = message;
            lBody.Text = lavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Special Method for North Point that removes multiple default groups by whether or not they have a grade or not. (Avoids children appearing in preschool default groups)
        /// </summary>
        /// <param name="person"></param>
        private void RemoveDefaultGroupsByGrade( CheckInPerson person )
        {
            //check if person has a grade
            var gradYear = person.Person.GradeOffset;
            var remove = GetAttributeValue( "Remove" ).AsBoolean();

            var selectedGroups = ( person.GroupTypes.SelectMany( gt => gt.Groups ).Where( g => g.ExcludedByFilter != true ) ).ToList();
            //if there are multiple default groups available, remove those without a grade criteria
            if ( selectedGroups.Count() > 1 )
            {
                List<CheckInGroup> excludeTheseGroups = new List<CheckInGroup>();
                foreach ( CheckInGroup group in selectedGroups )
                {
                    string gradeOffsetRange = group.Group.GetAttributeValue( "GradeRange" ) ?? string.Empty;
                    var gradeOffsetRangePair = gradeOffsetRange.Split( new char[] { ',' }, StringSplitOptions.None ).AsGuidOrNullList().ToArray();
                    DefinedValueCache minGradeDefinedValue = null;
                    DefinedValueCache maxGradeDefinedValue = null;
                    if ( gradeOffsetRangePair.Length == 2 )
                    {
                        minGradeDefinedValue = gradeOffsetRangePair[0].HasValue ? DefinedValueCache.Get( gradeOffsetRangePair[0].Value ) : null;
                        maxGradeDefinedValue = gradeOffsetRangePair[1].HasValue ? DefinedValueCache.Get( gradeOffsetRangePair[1].Value ) : null;
                    }

                    //If group has a GradeRange attached
                    if ( minGradeDefinedValue != null || maxGradeDefinedValue != null )
                    {   //And if person doesnt have a grade
                        if ( !gradYear.HasValue )
                        {
                            //group.ExcludedByFilter = true;
                            excludeTheseGroups.Add( group );
                        }
                    }
                    else
                    {
                        //If person does have a grade but the group doesnt
                        if ( gradYear.HasValue )
                        {
                            //group.ExcludedByFilter = true;
                            excludeTheseGroups.Add( group );
                        }
                    }
                }

                //if all groups were excluded, then do nothing (better everything than nothing)
                if ( excludeTheseGroups.Count() < selectedGroups.Count() )
                {
                    excludeTheseGroups.ForEach( g => g.ExcludedByFilter = true );
                }
            }
            
           
            //Remove empty group types
            if ( remove )
            {
                person.GroupTypes.RemoveAll( gt => !gt.Groups.Any() );
            }
            else
            {
                var groupTypesToExclude = person.GroupTypes.Where( gt => gt.Groups.All( g => g.ExcludedByFilter == true ) ).ToList();
                foreach ( CheckInGroupType groupType in groupTypesToExclude )
                {
                    groupType.ExcludedByFilter = true;
                }
            }


            SaveState();
        }

        /// <summary>
        /// Only Show Groups with the least amount of people checked into them, if they have the same Parent Group
        /// </summary>
        /// <param name="person"></param>
        private void LoadBalanceByParentGroup ( CheckInPerson person )
        {
            List<int?> parentGroupIds = person.GroupTypes.SelectMany( gt => gt.Groups ).Where( g => g.ExcludedByFilter != true && g.Group.ParentGroupId != null ).Select( g => g.Group.ParentGroupId ).Distinct().ToList();

            //for each parent group, see if there are more than one; then remove all but the lowest attended
            foreach( int? parentGroupId in parentGroupIds )
            {
                if ( parentGroupId == null ) //if id is null, skip it
                {
                    continue;
                }

                var checkinGroups = person.GroupTypes.SelectMany( gt => gt.Groups ).Where( g => g.ExcludedByFilter != true && g.Group.ParentGroupId == parentGroupId ).ToList();
                if ( checkinGroups.Count() > 1 )
                {
                    CheckInGroup lowestAttendedGroup = null;
                    int lowestAttendedCount = int.MaxValue;

                    //loop through each checkin group and find the one with the least attendance
                    foreach ( CheckInGroup group in checkinGroups )
                    {
                        var attendanceCount = 0;
                        foreach( int locationId in group.Locations.Select( l => l.Location.Id ) )
                        {
                            var attendanceCache = KioskLocationAttendance.Get( locationId );
                            if ( attendanceCache != null )
                            {
                                var attendanceGroup = attendanceCache.Groups.FirstOrDefault( g => g.GroupId == group.Group.Id );
                                if ( attendanceGroup != null )
                                {
                                    attendanceCount += attendanceGroup.CurrentCount;
                                }
                            }
                        }

                        if ( attendanceCount < lowestAttendedCount )
                        {
                            lowestAttendedCount = attendanceCount;
                            lowestAttendedGroup = group;
                        }
                    }

                    //exclude every checkinGroup but the lowestAttended
                    if ( lowestAttendedGroup != null )
                    {
                        foreach ( CheckInGroup group in checkinGroups )
                        {
                            if ( group.Group.Id != lowestAttendedGroup.Group.Id )
                            {
                                group.ExcludedByFilter = true;
                            }
                        }   
                    }
                    
                }
            }

            SaveState();
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }


        protected void lbSelect_Click( object sender, EventArgs e )
        {
            ProcessNextPersonDefaultGroups(); //this will navigate to next page when no more people are found
        }
    }
}