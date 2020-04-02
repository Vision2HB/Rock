using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace com_bemaservices.CheckIn.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that are not specific to their age or birthdate.
    /// </summary>
    [ActionCategory( "BEMA Services > Check-In" )]
    [Description( "If a group or set of groups are marked with an attribute like 'ExcludeOtherCriteriaGroups' set to 'Yes' then criteria based groups that do NOT have the attribute will be removed (or excluded) if a person is eligible for one or more groups with the attribute set to 'Yes.'" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Exclude Other Criteria Groups" )]

    [BooleanField ( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    [AttributeField ( Rock.SystemGuid.EntityType.GROUP, "Exclude Other Criteria Groups Attribute", "Select the attribute used to define the groups that will exclude other groups.", true, false,
         order: 2 )]
    public class FilterGroupsByExcludeOtherCriteriaGroups : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, Rock.Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                // Get the Exclude Attribute
                var excludeKey = string.Empty;
                var excludeAttributeGuid = GetAttributeValue ( action, "ExcludeOtherCriteriaGroupsAttribute" ).AsGuidOrNull ();
                if ( excludeAttributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get ( excludeAttributeGuid.Value, rockContext );
                    if ( attribute != null )
                    {
                        excludeKey = attribute.Key;
                    }
                }


                foreach ( var person in family.People )
                {
                    // If the person qualifies for Exclude Other Criteria Groups groups
                    if ( person.GroupTypes.SelectMany( t => t.Groups.Where(g => g.ExcludedByFilter != true).Select( g => g.Group ) ).Any( g => ( g.GetAttributeValue( excludeKey ).AsBooleanOrNull() ?? false ) == true ) )
                    {
                        // Remove all the criteria groups they qualified for that do not have the attribute
                        foreach ( var groupType in person.GroupTypes.Where( gt => gt.GroupType.AttendanceRule != Rock.Model.AttendanceRule.AlreadyBelongs ).ToList() )
                        {
                            foreach ( var group in groupType.Groups.Where ( g => g.ExcludedByFilter != true ).ToList () )
                            {
                                var keepGroup = group.Group.GetAttributeValue( excludeKey ).AsBooleanOrNull() ?? false;

                                if ( !keepGroup )
                                {
                                    if ( remove )
                                    {
                                        groupType.Groups.Remove( group );
                                    }
                                    else
                                    {
                                        group.ExcludedByFilter = true;
                                    }
                                }
                            }
                        }
                    }
                  
                }
            }

            return true;
        }
    }
}