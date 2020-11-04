using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Workflow.Action;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Data;
using Rock.Web.Cache;
using System.Data.Entity;
using System.ComponentModel;
using System.Text;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName( "Add Person" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Displays a list of families to select for checkin." )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Profile Source Attribute", "Choose a person attribute (single select) to place 'New Family Registration' in for new person", required: false, allowMultiple: false, key: "ProfileSourceAttribute" )]
    [WorkflowTypeField("New Child Workflows", "Starts a new workflow on successful new child, not new visitor. Available Lava Objects: 'ParentIds', 'ChildIds', Entity: the current family.", allowMultiple: true)]
    [WorkflowTypeField("New Visitor Workflows", "Starts a new workflow on successful new visitor, not new child. Available Lava Objects: 'ParentIds', 'ChildIds', Entity: the current family.", allowMultiple: true)]
    [CustomEnhancedListField( "Visitor Relationships", "Additional known relationships to add to the visiting child for the adults in selected family (Can Check-In By Automatically Added)", @"
SELECT 
	R.[Id] AS [Value],
	R.[Name] AS [Text]
FROM [GroupType] T
INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
AND R.[Name] <> 'Child'
UNION ALL
SELECT 0, 'Child'
ORDER BY [Text]", false, "", "Known Relationships", 3, "VisitorRelationships" )]
    public partial class AddPerson : CheckInBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                BindGrades();
                BindBirthDate();
                LabelBlocks();
            }
        }

        private void LabelBlocks()
        {
            lLegalGuardian.Text = "Are you the parent or legal guardian of this child, or are they visiting with you?";
            lLegalGuardianYes.Text = "My Child";
            lLegalGuardianNo.Text = "Visitor";
            lbParentBack.Text =  "Back";
            lbParentCancel.Text = "Cancel";
            lChildInformation.Text = "Child Information";
            
        }

        private void BindGrades()
        {
            var grades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() ).DefinedValues.Where( x => x.Value.AsInteger() <= 12 ).OrderBy( x => x.Order ).Select( x => new KeyValuePair<int?, string>( x.Value.AsInteger(), x.Description ) ).ToList();
            grades.Insert( 0, new KeyValuePair<int?, string>( 0, "Birth through Pre-K" ) );
            grades.Insert( 0, new KeyValuePair<int?, string>( null, "" ) );
            ddlGrade.DataSource = grades;
            ddlGrade.DataBind();
        }

        private void BindBirthDate()
        {
            rddlBirthMonth.DataSource = new List<string>() { "", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
            rddlBirthMonth.DataBind();
            rddlBirthDay.DataSource = new List<string>() { "", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };
            rddlBirthDay.DataBind();
            List<string> years = Enumerable.Range( 1950, DateTime.Today.Year - 1950 + 1 ).Reverse().ToList().ConvertAll<string>( x => x.ToString() );
            years.Insert( 0, "" );
            rddlBirthYear.DataSource = years;
            rddlBirthYear.DataBind();
        }

        private string ValidationMessage()
        {
            var sb = new StringBuilder();

            if ( string.IsNullOrWhiteSpace( tbFirstName.Text ) )
            {
                sb.Append( "<li>First Name" );
            }

            if ( string.IsNullOrWhiteSpace( tbLastName.Text ) )
            {
                sb.Append( "<li>Last Name" );
            }

            if ( !chkMale.Checked && !chkFemale.Checked )
            {
                sb.Append( "<li>Gender" );
            }

            if ( rddlBirthMonth.SelectedValue.IsNullOrWhiteSpace() || rddlBirthDay.SelectedValue.IsNullOrWhiteSpace() )
            {
                sb.Append( "<li>Birth Date" );
            }
            else if ( rddlBirthYear.SelectedValue.IsNullOrWhiteSpace() )
            {
                sb.Append( "<li>Birth Year" );
            }

            if ( ddlGrade.SelectedIndex <= 0 )
            {
                sb.Append( "<li>Age/Grade" );
            }

            if ( !string.IsNullOrWhiteSpace( sb.ToString() ) )
            {
                return "<P>Please provide the following:<ul>" + sb.ToString() + "</ul>";
            }

            return null;
        }

        protected void lbNext_Click( object sender, EventArgs e )
        {
            var validationMessage = ValidationMessage();
            if ( !string.IsNullOrEmpty( validationMessage ) )
            {
                maWarning.Show( validationMessage, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
            else
            {

                try
                {
                    if ( KioskCurrentlyActive )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var person = new Person();
                            bool isNewPerson = true;
                            //Try to match person
                            var personQuery = new PersonService.PersonMatchQuery( tbFirstName.Text.Trim(), tbLastName.Text.Trim(), null, null, chkMale.Checked ? Gender.Male : Gender.Female, DateTime.Parse( rddlBirthMonth.SelectedValue + "/" + rddlBirthDay.SelectedValue + "/" + rddlBirthYear.SelectedValue ) );
                            var foundPerson = new PersonService( rockContext ).FindPerson( personQuery, true );
                            if ( foundPerson.IsNotNull() && !hfSameFamily.Value.AsBoolean() ) //if found a person and only adding a 'visitor', not family child
                            {
                                person = foundPerson;
                                isNewPerson = false;
                            }
                            //End Try
                            else
                            {
                                person.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id;
                                person.FirstName = tbFirstName.Text;
                                person.LastName = tbLastName.Text;
                                person.Gender = chkMale.Checked ? Gender.Male : Gender.Female;
                                var birthDate = DateTime.Parse( rddlBirthMonth.SelectedValue + "/" + rddlBirthDay.SelectedValue + "/" + rddlBirthYear.SelectedValue );
                                person.SetBirthDate( birthDate );
                                person.ConnectionStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_ATTENDEE ).Id;
                            }
                            
                            
                            if ( !string.IsNullOrWhiteSpace( ddlGrade.SelectedValue ) && ddlGrade.SelectedValue != "0" )
                            {
                                person.GradeOffset = ddlGrade.SelectedValue.AsInteger();
                            }

                            if ( hfSameFamily.Value.AsBoolean() )
                            {
                                //Add New Child to Family (no special known relationships needed)
                                PersonService.AddPersonToFamily( person, true, CurrentCheckInState.CheckIn.CurrentFamily.Group.Id, ChildGroupRoleId, rockContext );
                            }
                            else
                            {
                                var familyGroup = PersonService.SaveNewPerson( person, rockContext );

                                var adults = Adults();

                                familyGroup.CampusId = GetKioskCampusId();
                                

                                // Set Can Check-In relationships for all adults in the current family
                                var visitorRelationshipIds = GetAttributeValue( "VisitorRelationships" ).SplitDelimitedValues().AsIntegerList();

                                foreach ( var adult in adults )
                                {
                                    var groupMemberService = new GroupMemberService( rockContext );

                                    //Add Person and Adult to a can check in known relationship
                                    var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
                                    var canCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ) );
                                    if ( canCheckInRole != null )
                                    {
                                        
                                        groupMemberService.CreateKnownRelationship( adult.PersonId, person.Id, canCheckInRole.Id );
                                    }

                                    //Add other visitor relationships with adults in family as needed (these are not removed by the block)
                                    foreach ( int relationshipId in visitorRelationshipIds )
                                    {
                                        groupMemberService.CreateKnownRelationship( adult.PersonId, person.Id, relationshipId );
                                    }
                                }

                            }

                            rockContext.SaveChanges();

                            person.LoadAttributes();
                            // Save the attributes after saving the person
                            if ( !string.IsNullOrWhiteSpace( tbAllergy.Text ) )
                            {
                                
                                person.SetAttributeValue( "Allergy", tbAllergy.Text );
                                //person.SaveAttributeValue( "Allergy", rockContext );
                            }
                            if ( !string.IsNullOrWhiteSpace( tbMedical.Text ) )
                            {
                                person.SetAttributeValue( "Arena-16-81", tbMedical.Text );
                                //person.SaveAttributeValue( "Arena-16-81", rockContext );
                            }
                            if ( !string.IsNullOrWhiteSpace( tbLegal.Text ) )
                            {
                                person.SetAttributeValue( "LegalNotes", tbLegal.Text );
                                //person.SaveAttributeValue( "LegalNotes", rockContext );
                            }

                            //if NPM Profile Source, add 'New Family Registration iPad'
                            if (GetAttributeValue( "ProfileSourceAttribute" ).AsGuidOrNull().HasValue && isNewPerson )
                            {
                                string key = AttributeCache.Get( GetAttributeValue( "ProfileSourceAttribute" ).AsGuid() ).Key;
                                if ( person.GetAttributeValue( key ).IsNullOrWhiteSpace() )
                                {
                                    if (hfSameFamily.Value.AsBoolean())
                                    {
                                        person.SetAttributeValue( key, "Add Child at Kiosk" );
                                    }
                                    else
                                    {
                                        person.SetAttributeValue( key, "Add Visitor at Kiosk" );
                                    }
                                        
                                }
                                //person.SaveAttributeValue( key, rockContext );
                            }
                            person.SaveAttributeValues( rockContext );

                            //rehydrate person
                            person = new PersonService( new RockContext() ).Get( person.Id );

                            //Look for any child or visitor workflows
                            List<Guid> newChildWorkflows = GetAttributeValue( "NewChildWorkflows" ).SplitDelimitedValues().AsGuidList();
                            List<Guid> newVisitorWorkflows = GetAttributeValue( "NewVisitorWorkflows" ).SplitDelimitedValues().AsGuidList();
                            if ( newChildWorkflows.Any() || newVisitorWorkflows.Any() )
                            {
                                var adults = Adults();
                                // Create parameters
                                var parameters = new Dictionary<string, string>();
                                parameters.Add( "ParentIds", adults.Select( gm => gm.PersonId).ToList().AsDelimited( "," ) );
                                parameters.Add( "ChildIds", person.Id.ToString() );
                                parameters.Add( "CampusId", GetKioskCampusId().ToString() );

                                // Look for any child workflows
                                if ( hfSameFamily.Value.AsBoolean() && newChildWorkflows.Any() )
                                {
                                    // Launch all the workflows
                                    foreach ( var wfGuid in newChildWorkflows )
                                    {
                                        person.PrimaryFamily.LaunchWorkflow( wfGuid, person.PrimaryFamily.Name, parameters );
                                    }
                                }

                                //Look for any visitor workflows
                                if ( !hfSameFamily.Value.AsBoolean() && newVisitorWorkflows.Any() )
                                {
                                    // Launch all the workflows
                                    foreach ( var wfGuid in newVisitorWorkflows )
                                    {
                                        adults.FirstOrDefault().Person.PrimaryFamily.LaunchWorkflow( wfGuid, person.PrimaryFamily.Name, parameters );
                                    }
                                }

                            }

                            // execute the workflow activity that is configured for this block (probably 'Person Search') so that
                            // the checkin state gets updated with any changes we made in Edit Family
                            //string workflowActivity = GetAttributeValue( "WorkflowActivity" );
                            string workflowActivity = "Person Search";  //manually set to 'PersonSearch' 
                            List<string> errorMessages;
                            if ( !string.IsNullOrEmpty( workflowActivity ) )
                            {
                                ProcessActivity( workflowActivity, out errorMessages );
                            }


                            ProcessSelection( maWarning );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    while ( ex.InnerException != null )
                    {
                        ex = ex.InnerException;
                    }
                    ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
                }
            }
        }

        private int? GetKioskCampusId()
        {
            if ( CurrentCheckInState.Kiosk.CampusId.HasValue )
            {
                return CurrentCheckInState.Kiosk.CampusId.Value;
            }
            else if( CurrentCheckInState.Kiosk.Device.Locations.FirstOrDefault().IsNotNull() )
            {
                return CurrentCheckInState.Kiosk.Device.Locations.FirstOrDefault().CampusId;
            }
            else
            {
                return new CampusService( new RockContext() ).Queryable().AsNoTracking().OrderBy( c => c.Order ).First().Id;
            }

        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        protected void lbYes_Click( object sender, EventArgs e )
        {
            hfSameFamily.Value = true.ToString();

            // If all the adults on the family share a last name, populate the last name field
            var adultLastNames = Adults().Select(x => x.Person.LastName).Distinct();
            if (adultLastNames.Count() == 1)
            {
                tbLastName.Text = adultLastNames.FirstOrDefault();
            }

            pnlParent.Visible = false;
            pnlChild.Visible = true;
            SaveState();
        }

        private List<GroupMember> Adults()
        {
            var qGroupMemberService = new GroupMemberService( new RockContext() ).Queryable().AsNoTracking();
            return qGroupMemberService.Where( x => x.GroupId == CurrentCheckInState.CheckIn.CurrentFamily.Group.Id && x.GroupRoleId == AdultGroupRoleId ).ToList();
        }

        protected void lbNo_Click( object sender, EventArgs e )
        {
            hfSameFamily.Value = false.ToString();
            pnlParent.Visible = false;
            pnlChild.Visible = true;
            SaveState();
        }

        protected void lbParentBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbParentCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        private GroupTypeCache FamilyGroupType
        {
            get
            {
                return GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            }
        }

        private int ChildGroupRoleId
        {
            get
            {
                return FamilyGroupType.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;
            }
        }

        private int AdultGroupRoleId
        {
            get
            {
                return FamilyGroupType.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            }
        }

        protected void chkMale_CheckedChanged( object sender, EventArgs e )
        {
            cblGender.SelectedValue = "Male";
            chkMale.Checked = true; // Do not allow unchecking
            chkFemale.Checked = false;
        }

        protected void chkFemale_CheckedChanged( object sender, EventArgs e )
        {
            cblGender.SelectedValue = "Female";
            chkFemale.Checked = true; // Do not allow unchecking
            chkMale.Checked = false;
        }
    }
}