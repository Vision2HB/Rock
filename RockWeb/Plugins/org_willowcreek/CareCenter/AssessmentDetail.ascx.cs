using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing/editing details of a assessment.
    /// </summary>
    [DisplayName( "Assessment Detail" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing/editing details of a assessment." )]

    [LinkedPage("Workflow Entry Page", "Page to direct user to when they click a service.", true, "", "", 0)]
    public partial class AssessmentDetail : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.lazyload.min.js" ) );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );


            gWorkflows.DataKeyNames = new string[] { "Id" };
            gWorkflows.Actions.ShowAdd = true;
            gWorkflows.Actions.AddClick += Actions_AddClick;
            gWorkflows.RowDataBound += gWorkflows_RowDataBound;
            gWorkflows.GridRebind += gWorkflows_GridRebind;
            gWorkflows.IsDeleteEnabled = true;
            gWorkflows.ShowConfirmDeleteDialog = false;

            lbEdit.Visible = UserCanEdit;

            string cancelButtonScript = @"
$('#{0} .assessment-cancel-button').not('.disabled').on( 'click', function (e) {{
    e.preventDefault();
    Rock.dialogs.confirm('Are you sure you want to cancel this ' + $(this).attr('data-assessment-type') + '?', function (result) {{
        if (result) {{
            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
        }}
    }});
}});";
            string deleteButtonScript = string.Format( cancelButtonScript, gWorkflows.ClientID );
            ScriptManager.RegisterStartupScript( gWorkflows, gWorkflows.GetType(), "assessment-cancel-confirm-" + this.ClientID, deleteButtonScript, true );
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbWarningMessage.Visible = false;
            nbServerValidation.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "AssessmentId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int assessmentId = hfAssessmentId.ValueAsInt();
            ShowReadonlyDetails( GetAssessment( assessmentId ) );
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetAssessment( hfAssessmentId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var assessmentService = new AssessmentService( rockContext );
                Assessment assessment = null;

                int? assessmentId = hfAssessmentId.Value.AsIntegerOrNull();
                if ( assessmentId.HasValue )
                {
                    assessment = assessmentService.Get( assessmentId.Value );
                }

                if ( assessment == null )
                {
                    nbServerValidation.Heading = "Save Error";
                    nbServerValidation.Text = "The assessment record could not be found. It may have been deleted while you were editing the values.";
                    nbServerValidation.Visible = true;
                    return;
                }

                assessment.PersonAliasId = ppPrimaryContact.PersonAliasId ?? 0;
                assessment.ApprovedByPersonAliasId = ppApprovedBy.PersonAliasId;
                assessment.AssessmentDate = dtAssessmentDate.SelectedDateTime ?? RockDateTime.Now;

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !assessment.IsValid )
                {
                    nbServerValidation.Heading = string.Empty;
                    nbServerValidation.Text = string.Format( "Please correct the following:<ul><li>{0}</li></ul", assessment.ValidationResults.AsDelimited( "</li><li>" ) );
                    nbServerValidation.Visible = true;
                    return;
                }

                rockContext.SaveChanges();

                hfAssessmentId.SetValue( assessment.Id );

                // Requery the batch to support EF navigation properties
                var savedAssessment = GetAssessment( assessment.Id );
                ShowReadonlyDetails( savedAssessment );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int assessmentId = hfAssessmentId.ValueAsInt();
            ShowReadonlyDetails( GetAssessment( assessmentId ) );
        }

        private void Actions_AddClick( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string> { { "AssessmentId", hfAssessmentId.Value } };
            NavigateToLinkedPage( "IntakePage", parms );
        }


        /// <summary>
        /// Handles the RowDataBound event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gWorkflows_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var workflow = e.Row.DataItem as Workflow;
                if ( workflow != null )
                {
                    var lStatus = e.Row.FindControl( "lStatus" ) as Literal;
                    if ( lStatus != null )
                    {
                        string labelClass = "default";
                        switch ( workflow.Status )
                        {
                            case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_PENDING: labelClass = "default"; break;
                            case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_WAITING: labelClass = "success"; break;
                            case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_INPROGRESS: labelClass = "success"; break;
                            case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_COMPLETED: labelClass = "default"; break;
                            case org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED: labelClass = "danger"; break;
                            default: labelClass = "default"; break;
                        }

                        lStatus.Text = string.Format( "<span class='label label-{0}'>{1}</span>", labelClass, workflow.Status );
                    }

                    var lServiceHistory = e.Row.FindControl( "lServiceHistory" ) as Literal;
                    if ( lServiceHistory != null )
                    {
                        var activities = workflow.Activities
                            .Where( a => a.ActivatedDateTime.HasValue )
                            .OrderByDescending( a => a.ActivatedDateTime.Value )
                            .Select( a => new
                            {
                                a.ActivatedDateTime,
                                Name = a.ActivityType.Name
                            } );

                        if ( activities.Any() )
                        {
                            var activityText = new StringBuilder();
                            foreach ( var activity in activities )
                            {
                                string labelClass = "default";
                                switch ( activity.Name )
                                {
                                    case "Waiting":
                                    case "In Progress":
                                        labelClass = "success";
                                        break;
                                    case "Cancelled":
                                        labelClass = "danger";
                                        break;
                                    default:
                                        labelClass = "default";
                                        break;
                                }
                                string label = string.Format( "<span class='label label-{0}'>{1}</span>", labelClass, activity.Name );
                                activityText.AppendFormat( "<div class=''>{0} {1}</div>", activity.ActivatedDateTime.Value.ToShortTimeString(), label );
                            }
                            lServiceHistory.Text = activityText.ToString();
                        }
                        else
                        {
                            lServiceHistory.Text = string.Empty;
                        }
                    }

                    foreach ( var lb in e.Row.ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lb.Attributes["data-assessment-type"] = workflow.WorkflowType.Name;
                    }

                    if ( !workflow.IsActive )
                    {
                        e.Row.Cells[e.Row.Cells.Count - 1].Controls.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindWorkflowsGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflows_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflow = new WorkflowService( rockContext ).Get( e.RowKeyId );
                if ( workflow != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "WorkflowTypeId", workflow.WorkflowTypeId.ToString() );
                    qryParams.Add( "WorkflowId", workflow.Id.ToString() );
                    NavigateToLinkedPage( "WorkflowEntryPage", qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflows_Delete( object sender, RowEventArgs e )
        {
            int? assessmentId = hfAssessmentId.Value.AsIntegerOrNull();

            using ( var rockContext = new RockContext() )
            {
                var workflow = new WorkflowService( rockContext ).Get( e.RowKeyId );
                if ( workflow != null )
                {
                    workflow.AddLogEntry( "Workflow Cancelled" );
                    workflow.CompletedDateTime = RockDateTime.Now;
                    workflow.Status = org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED;

                    rockContext.SaveChanges();
                }
            }

            ShowDetail( assessmentId ?? 0 );
        }

        #endregion

        #region Methods

        private Assessment GetAssessment( int assessmentId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var assessment = new AssessmentService( rockContext ).Get( assessmentId );
            return assessment;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="assessmentId">The financial assessment identifier.</param>
        public void ShowDetail( int assessmentId )
        {
            Assessment assessment = GetAssessment( assessmentId );
            if ( assessment == null )
            {
                nbWarningMessage.Text = "Could not find selected Assessment.";
                nbWarningMessage.Visible = true;
                SetEditMode( null );
            }
            else
            {
                hfAssessmentId.Value = assessment.Id.ToString();
                lbEdit.Visible = IsUserAuthorized( Authorization.EDIT );
                ShowReadonlyDetails( assessment );
            }
        }

        /// <summary>
        /// Shows the financial assessment summary.
        /// </summary>
        /// <param name="assessment">The financial assessment.</param>
        private void ShowReadonlyDetails( Assessment assessment )
        {
            if ( assessment == null )
            {
                SetEditMode( null );
            }
            else
            {
                SetEditMode( false );

                lPhoto.Text = string.Format( "<div class=\"photo-round photo-round-sm pull-left margin-r-sm margin-t-sm\" data-original=\"{0}&w=120\" style=\"background-image: url('{1}');\"></div>", assessment.PersonAlias.Person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-male.svg" ) );
                lPrimaryContact.Text = assessment.PersonAlias != null && assessment.PersonAlias.Person != null ? assessment.PersonAlias.Person.FullName : "";
                lApprovedBy.Text = assessment.ApprovedByPersonAlias != null && assessment.ApprovedByPersonAlias.Person != null ? assessment.ApprovedByPersonAlias.Person.FullName : "";
                lDate.Text = assessment.AssessmentDate.ToString();

                BindWorkflowsGrid();
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="assessment">The financial assessment.</param>
        protected void ShowEditDetails( Assessment assessment )
        {
            if ( assessment == null )
            {
                SetEditMode( null );
            }
            else
            {
                SetEditMode( true );

                if ( assessment.PersonAlias != null && assessment.PersonAlias.Person != null )
                {
                    ppPrimaryContact.PersonId = assessment.PersonAlias.Person.Id;
                    ppPrimaryContact.PersonName = assessment.PersonAlias.Person.FullName;
                }
                else
                {
                    ppPrimaryContact.PersonId = null;
                    ppPrimaryContact.PersonName = string.Empty;
                }

                if ( assessment.ApprovedByPersonAlias != null && assessment.ApprovedByPersonAlias.Person != null )
                {
                    ppApprovedBy.PersonId = assessment.ApprovedByPersonAlias.Person.Id;
                    ppApprovedBy.PersonName = assessment.ApprovedByPersonAlias.Person.FullName;
                }
                else
                {
                    ppApprovedBy.PersonId = null;
                    ppApprovedBy.PersonName = string.Empty;
                }

                dtAssessmentDate.SelectedDateTime = assessment.AssessmentDate;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool? editable )
        {
            pnlEditDetails.Visible = editable.HasValue && editable.Value;
            pnlViewDetails.Visible = editable.HasValue && !editable.Value;

            this.HideSecondaryBlocks( editable.HasValue && editable.Value );
        }

        private void BindWorkflowsGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                int? assessmentId = hfAssessmentId.Value.AsIntegerOrNull();
                if ( assessmentId.HasValue )
                {
                    var assessment = new AssessmentService( rockContext ).Get( assessmentId.Value );

                    gWorkflows.DataSource = assessment.Workflows.OrderBy( w => w.WorkflowType.Order ).ToList();
                    gWorkflows.DataBind();
                    gWorkflows.Visible = true;
                }
                else
                {
                    gWorkflows.Visible = false;
                }
            }
        }

        #endregion


    }
}