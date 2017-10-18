using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing list of visits.
    /// </summary>
    [DisplayName( "Dashboard Workflow" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Block to display button for returning to dashboard from the workflow form page." )]

    [KeyValueListField("Workflow Type Dashboard Page", "The Workflow Types and their cooresponding Page Ids", false, "", "Workflow Type Id", "Page Id", "", "", "", 0 )]
    [ValueListField("Workflow Type Add Appointment", "The Workflow types that should allow adding an appointment", false, "", "Workflow Type Id", "", "", "", 1)]
    [LinkedPage("Appointment Add Page", "Page used to add a new appointment", false, "", "", 2)]
    [TextField( "Browser Title", "Optional text to use for overriding the default broswer title. For example: {{ Workflow.Name }} ({{ Workflow.WorkflowType.Name }}) <span class='tip tip-lava'></span>.", false, "", "", 3 )]
    public partial class DashboardWorkflow : Rock.Web.UI.RockBlock
    {
        int? _workflowId = null;
        int? _workflowTypeId = null;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _workflowId = PageParameter( "WorkflowId" ).AsIntegerOrNull();
            _workflowTypeId = PageParameter( "WorkflowTypeId" ).AsIntegerOrNull();

            if ( _workflowId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflow = new WorkflowService( rockContext ).Get( _workflowId.Value );
                    _workflowTypeId = workflow != null ? workflow.WorkflowTypeId : (int?)null;

                    string browserTitle = GetAttributeValue( "BrowserTitle" );
                    if ( browserTitle.IsNotNullOrWhitespace() )
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                        mergeFields.Add( "Workflow", workflow );
                        RockPage.BrowserTitle = browserTitle.ResolveMergeFields( mergeFields );
                    }
                }
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( _workflowTypeId.HasValue )
            {
                var pageIds = GetAttributeValue( "WorkflowTypeDashboardPage" );
                if ( pageIds.IsNotNullOrWhitespace() )
                {
                    hfDashboardPageId.Value = pageIds.TrimEnd( '|' ).Split( '|' )
                        .Select( s => s.Split( '^' ) )
                        .Where( p => p[0] == _workflowTypeId.Value.ToString() )
                        .Select( p => p[1] )
                        .FirstOrDefault();
                }
                btnDashboard.Visible = hfDashboardPageId.Value.AsIntegerOrNull().HasValue;

                if ( GetAttributeValue( "AppointmentAddPage" ).IsNotNullOrWhitespace() )
                {
                    var apptWorkflowTypeIds = GetAttributeValue( "WorkflowTypeAddAppointment" );
                    if ( apptWorkflowTypeIds.IsNotNullOrWhitespace() )
                    {
                        if ( apptWorkflowTypeIds.TrimEnd( '|' ).Split( '|' ).AsIntegerList().Contains( _workflowTypeId.Value ) )
                        {
                            hlAddAppointment.Visible = true;

                            if ( _workflowId.HasValue )
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    var workflow = new WorkflowService( rockContext ).Get( _workflowId.Value );
                                    if ( workflow != null )
                                    {
                                        workflow.LoadAttributes();
                                        Guid? personAliasGuid = workflow.GetAttributeValue( "Person" ).AsGuidOrNull();
                                        if ( personAliasGuid.HasValue )
                                        {
                                            var person = new PersonAliasService( rockContext ).GetPerson( personAliasGuid.Value );
                                            if ( person != null )
                                            {
                                                hlAddAppointment.NavigateUrl = LinkedPageUrl( "AppointmentAddPage", new Dictionary<string, string> { { "PersonId", person.Id.ToString() } } );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            hlAddAppointment.Visible = false;
                        }
                    }
                }
            }
        }

        protected void btnDashboard_Click( object sender, EventArgs e )
        {
            int? pageId = hfDashboardPageId.Value.AsIntegerOrNull();
            if ( pageId.HasValue )
            {
                NavigateToPage( new PageReference( pageId.Value ) );
            }
        }

    }
}