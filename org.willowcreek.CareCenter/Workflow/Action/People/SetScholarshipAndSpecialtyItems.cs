using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using org.willowcreek.CareCenter.Web.UI.Controls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace org.willowcreek.CareCenter.Workflow.Action
{
    /// <summary>
    /// Sets scholarship and specialty items.
    /// </summary>
    /// <seealso cref="Rock.Workflow.ActionComponent" />
    [ActionCategory( "People" )]
    [Description( "Sets scholarship and specialty items." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Scholarship and Specialty Items" )]

    [TextField( "Family Scholarship Attribute Key", "The Family Attribute Key to update with new scholarship amount.", false, "ScholarshipAmount", "", 0 )]
    public class SetScholarshipAndSpecialtyItems : ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( action != null && action.Activity != null && action.Activity.Workflow != null )
            {
                // Set Scholarhip Amount User
                var amountUsed = MaxCurrency.MaxAndValue.FromJson( action.Activity.Workflow.GetAttributeValue( "UseScholarshipReward" ) );
                if ( amountUsed != null && amountUsed.Value.HasValue && amountUsed.Value.Value > 0.0M )
                {
                    var familyGuid = action.Activity.Workflow.GetAttributeValue( "Family" ).AsGuidOrNull();
                    if ( familyGuid.HasValue )
                    {
                        var family = new GroupService( rockContext ).Get( familyGuid.Value );
                        if ( family != null )
                        {
                            family.LoadAttributes();

                            string attributeKey = GetAttributeValue( action, "FamilyScholarshipAttributeKey" );

                            if ( family.Attributes.ContainsKey( attributeKey ) )
                            {
                                var attribute = family.Attributes[attributeKey];
                                var existingAmount = family.GetAttributeValue( attributeKey ).AsDecimal();
                                var newAmount = existingAmount - amountUsed.Value;
                                if ( newAmount < 0.0M )
                                {
                                    newAmount = 0.0M;
                                }

                                if ( newAmount != existingAmount )
                                {
                                    Rock.Attribute.Helper.SaveAttributeValue( family, attribute, newAmount.ToString(), rockContext );
                                }
                            }
                        }
                    }
                }

                // Set Specialty Items
                var specialtyItems = PeopleValuesSelected.PeopleAndValues.FromJson( action.Activity.Workflow.GetAttributeValue( "SpecialtyItems" ) );
                if ( specialtyItems != null )
                {
                    string dateValue = RockDateTime.Now.ToString( "o" );
                    foreach ( var child in specialtyItems.People )
                    {
                        if ( child.SelectedValues != null && child.SelectedValues.Any() )
                        {
                            var person = new PersonService( rockContext ).Get( child.PersonGuid );
                            if ( person != null )
                            {
                                person.LoadAttributes();
                                foreach( var item in child.SelectedValues )
                                {
                                    Guid? guid = item.AsGuidOrNull();
                                    if ( guid.HasValue )
                                    {
                                        var attribute = AttributeCache.Read( guid.Value );
                                        if ( attribute != null )
                                        {
                                            Rock.Attribute.Helper.SaveAttributeValue( person, attribute, dateValue.ToString(), rockContext );
                                        }
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
