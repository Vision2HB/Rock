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
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing/editing details of a visit.
    /// </summary>
    [DisplayName( "Visit Detail" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing/editing details of a visit." )]

    [LinkedPage("Workflow Entry Page", "Page to direct user to when they click a service.", true, "", "", 0)]
    [LinkedPage( "Intake Page", "Page to direct user to when they add a new service.", true, "", "", 1 )]
    [LinkedPage( "Passport Page", "Page to view and print a Passport.", true, "", "", 2 )]
    public partial class VisitDetail : Rock.Web.UI.RockBlock
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


            gServices.DataKeyNames = new string[] { "Id" };
            gServices.Actions.ShowAdd = true;
            gServices.Actions.AddClick += Actions_AddClick;
            gServices.RowDataBound += gServices_RowDataBound;
            gServices.GridRebind += gServices_GridRebind;
            gServices.IsDeleteEnabled = true;
            gServices.ShowConfirmDeleteDialog = false;

            gWorkflows.DataKeyNames = new string[] { "Id" };
            gWorkflows.Actions.ShowAdd = true;
            gWorkflows.Actions.AddClick += Actions_AddClick;
            gWorkflows.RowDataBound += gServices_RowDataBound;
            gWorkflows.GridRebind += gWorkflows_GridRebind;
            gWorkflows.IsDeleteEnabled = true;
            gWorkflows.ShowConfirmDeleteDialog = false;

            lbEdit.Visible = UserCanEdit;

            lbCancelVisit.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'This will cancel all of the service areas! Are you sure you want to continue?');", EventCalendar.FriendlyTypeName );

            string cancelButtonScript = @"
$('#{0} .visit-cancel-button').not('.disabled').on( 'click', function (e) {{
    e.preventDefault();
    Rock.dialogs.confirm('Are you sure you want to cancel this ' + $(this).attr('data-visit-type') + '?', function (result) {{
        if (result) {{
            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
        }}
    }});
}});";
            string deleteButtonScript = string.Format( cancelButtonScript, gServices.ClientID );
            ScriptManager.RegisterStartupScript( gServices, gServices.GetType(), "visit-cancel-confirm-" + this.ClientID, deleteButtonScript, true );
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
                ShowDetail( PageParameter( "VisitId" ).AsInteger() );
            }
            else
            {
                ShowDialog();
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
            int visitId = hfVisitId.ValueAsInt();
            ShowReadonlyDetails( GetVisit( visitId ) );
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetVisit( hfVisitId.Value.AsInteger() ) );
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
                var visitService = new VisitService( rockContext );
                Visit visit = null;

                int? visitId = hfVisitId.Value.AsIntegerOrNull();
                if ( visitId.HasValue )
                {
                    visit = visitService.Get( visitId.Value );
                }

                if ( visit == null )
                {
                    nbServerValidation.Heading = "Save Error";
                    nbServerValidation.Text = "The visit record could not be found. It may have been deleted while you were editing the values.";
                    nbServerValidation.Visible = true;
                    return;
                }

                visit.PersonAliasId = ppPrimaryContact.PersonAliasId ?? 0;
                visit.Status = ddlStatus.SelectedValueAsEnum<VisitStatus>();
                visit.CancelReasonValueId = ddlCancelReason.SelectedValueAsInt();
                visit.VisitDate = dtVisitDate.SelectedDateTime ?? RockDateTime.Now;
                visit.PagerId = nbPagerId.Text.AsIntegerOrNull();
                visit.PassportStatus = cbPassportPrinted.Checked ? PassportStatus.Printed : PassportStatus.NotPrinted;

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !visit.IsValid )
                {
                    nbServerValidation.Heading = string.Empty;
                    nbServerValidation.Text = string.Format( "Please correct the following:<ul><li>{0}</li></ul", visit.ValidationResults.AsDelimited( "</li><li>" ) );
                    nbServerValidation.Visible = true;
                    return;
                }

                rockContext.SaveChanges();

                visitService.UpdateWorkflowPassportStatus( visit.Id );

                hfVisitId.SetValue( visit.Id );

                // Requery the batch to support EF navigation properties
                var savedVisit = GetVisit( visit.Id );
                ShowReadonlyDetails( savedVisit );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int visitId = hfVisitId.ValueAsInt();
            ShowReadonlyDetails( GetVisit( visitId ) );
        }

        /// <summary>
        /// Handles the Click event of the lbCancelVisit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelVisit_Click( object sender, EventArgs e )
        {
            ddlDlgCancelReason.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.VISIT_CANCEL_REASON.AsGuid() ), true );
            tbCancelNote.Text = string.Empty;
            ShowDialog( "Cancel" );
        }

        private void Actions_AddClick( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string> { { "VisitId", hfVisitId.Value } };
            NavigateToLinkedPage( "IntakePage", parms );
        }


        /// <summary>
        /// Handles the RowDataBound event of the gServices control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gServices_RowDataBound( object sender, GridViewRowEventArgs e )
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
                        lb.Attributes["data-visit-type"] = workflow.WorkflowTypeCache.Name;
                    }

                    if ( !workflow.IsActive )
                    {
                        e.Row.Cells[e.Row.Cells.Count - 1].Controls.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gServices control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gServices_GridRebind( object sender, EventArgs e )
        {
            BindServicesGrid();
        }

        private void gWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindWorkflowsGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gServices control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gServices_RowSelected( object sender, RowEventArgs e )
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
        /// Handles the Delete event of the gServices control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gServices_Delete( object sender, RowEventArgs e )
        {
            hfWorkflowId.Value = e.RowKeyId.ToString();
            ddlDlgCancelWorkflowReason.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.VISIT_CANCEL_REASON.AsGuid() ), true );
            ShowDialog( "CancelWorkFlow" );
        }

        protected void dlgCancelWorkflow_SaveClick( object sender, EventArgs e )
        {
            int? visitId = hfVisitId.Value.AsIntegerOrNull();

            using ( var rockContext = new RockContext() )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflow = workflowService.Get( hfWorkflowId.Value.AsInteger() );
                if ( workflow != null )
                {
                    workflow.LoadAttributes( rockContext );
                    Guid? nextWorkflowGuid = workflow.GetAttributeValue( "NextWorkflow" ).AsGuidOrNull();

                    var definedValue = DefinedValueCache.Read( ddlDlgCancelWorkflowReason.SelectedValueAsInt() ?? 0 );
                    if ( definedValue != null )
                    {
                        workflow.SetAttributeValue( "CancelReason", definedValue.Guid.ToString().ToLower() );
                    }

                    workflow.AddLogEntry( "Service Area Cancelled" );
                    workflow.CompletedDateTime = RockDateTime.Now;
                    workflow.Status = org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED;

                    rockContext.SaveChanges();
                    workflow.SaveAttributeValues( rockContext );

                    if ( nextWorkflowGuid.HasValue )
                    {
                        var nextWorkflow = workflowService.Get( nextWorkflowGuid.Value );
                        if ( nextWorkflow != null && 
                            nextWorkflow.Status == "Pending" &&
                            !nextWorkflow.CompletedDateTime.HasValue )
                        {
                            nextWorkflow.Status = "Waiting";
                            List<string> workflowErrors;
                            workflowService.Process( nextWorkflow, out workflowErrors );
                        }
                    }

                    // If all the workflows have been completed for this visit...
                    var visit = new VisitService( rockContext ).Queryable()
                        .Where( v =>
                            v.Id == visitId.Value &&
                            !v.Workflows.Any( w => w.CompletedDateTime == null ) )
                        .FirstOrDefault();
                    if ( visit != null )
                    {
                        // check to see if any do not have a status of Cancelled
                        if ( visit.Workflows.Any( w => w.Status != org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED ) )
                        {
                            // If so, mark the visit complete
                            visit.Status = VisitStatus.Complete;
                        }
                        else
                        {
                            // otherwise mark it cancelled
                            visit.Status = VisitStatus.Cancelled;
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            HideDialog();

            ShowDetail( visitId ?? 0 );
        }

        protected void gWorkflows_Delete( object sender, RowEventArgs e )
        {
            int? visitId = hfVisitId.Value.AsIntegerOrNull();

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

            ShowDetail( visitId ?? 0 );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowCancelReason();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgCancel_SaveClick( object sender, EventArgs e )
        {
            int? visitId = hfVisitId.Value.AsIntegerOrNull();

            using ( var rockContext = new RockContext() )
            {
                var visitService = new VisitService( rockContext );
                if ( visitId.HasValue )
                {
                    var visit = visitService.Get( visitId.Value );
                    if ( visit != null )
                    {
                        var now = RockDateTime.Now;
                        foreach( var wf in visit.Workflows )
                        {
                            wf.AddLogEntry( "Visit Cancelled" );
                            wf.CompletedDateTime = now;
                            wf.Status = org.willowcreek.CareCenter.Constant.WORKFLOW_STATUS_CANCELLED;
                        }

                        visit.Status = VisitStatus.Cancelled;
                        visit.CancelReasonValueId = ddlDlgCancelReason.SelectedValueAsInt();

                        if ( !string.IsNullOrWhiteSpace( tbCancelNote.Text ) )
                        {
                            var noteType = NoteTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.NoteType.VISIT_NOTES.AsGuid() );
                            if ( noteType != null )
                            {
                                var note = new Note();
                                note.NoteTypeId = noteType.Id;
                                note.EntityId = visit.Id;
                                note.Text = tbCancelNote.Text;
                                new NoteService( rockContext ).Add( note );
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            ShowDetail( visitId ?? 0 );

            HideDialog();
        }

        #endregion

        #region Methods

        private Visit GetVisit( int visitId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var visit = new VisitService( rockContext ).Get( visitId );
            return visit;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="visitId">The financial visit identifier.</param>
        public void ShowDetail( int visitId )
        {
            ddlStatus.BindToEnum<VisitStatus>();
            ddlCancelReason.BindToDefinedType( DefinedTypeCache.Read( org.willowcreek.CareCenter.SystemGuid.DefinedType.VISIT_CANCEL_REASON.AsGuid() ) );

            Visit visit = GetVisit( visitId );
            if ( visit == null )
            {
                nbWarningMessage.Text = "Could not find selected Visit.";
                nbWarningMessage.Visible = true;
                SetEditMode( null );
            }
            else
            {
                hfVisitId.Value = visit.Id.ToString();
                lbEdit.Visible = IsUserAuthorized( Authorization.EDIT );
                ShowReadonlyDetails( visit );
            }
        }

        /// <summary>
        /// Shows the financial visit summary.
        /// </summary>
        /// <param name="visit">The financial visit.</param>
        private void ShowReadonlyDetails( Visit visit )
        {
            if ( visit == null )
            {
                SetEditMode( null );
            }
            else
            {
                SetEditMode( false );

                SetHeadingInfo( visit );
                lPhoto.Text = string.Format( "<div class=\"photo-round photo-round-sm pull-left margin-r-sm margin-t-sm\" data-original=\"{0}&w=120\" style=\"background-image: url('{1}');\"></div>", visit.PersonAlias.Person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-male.svg" ) );
                lPrimaryContact.Text = visit.PersonAlias.Person.FullName;
                lPager.Text = visit.PagerId.HasValue ? visit.PagerId.Value.ToString() : "";
                lDate.Text = visit.VisitDate.ToString();

                lCreatedBy.Text = visit.CreatedByPersonAlias != null && visit.CreatedByPersonAlias.Person != null ?
                    visit.CreatedByPersonAlias.Person.FullName : string.Empty;

                lbCancelVisit.Visible = visit.Status == VisitStatus.Active;

                hlPrintPassport.Visible = visit.Status == VisitStatus.Active && visit.PassportStatus != PassportStatus.NotNeeded;
                hlPrintPassport.CssClass = visit.PassportStatus == PassportStatus.Printed ? "btn btn-default" : "btn btn-primary";
                hlPrintPassport.Text = visit.PassportStatus == PassportStatus.Printed ? "Reprint Passport" : "Print Passport";
                hlPrintPassport.NavigateUrl = LinkedPageUrl( "PassportPage", new Dictionary<string, string> { { "VisitIds", visit.Id.ToString() } } );

                BindServicesGrid();
                BindWorkflowsGrid();
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="visit">The financial visit.</param>
        protected void ShowEditDetails( Visit visit )
        {
            if ( visit == null )
            {
                SetEditMode( null );
            }
            else
            {
                SetEditMode( true );

                ppPrimaryContact.PersonId = visit.PersonAlias.PersonId;
                ppPrimaryContact.PersonName = visit.PersonAlias.Person.FullName;

                ddlStatus.SetValue( Convert.ToInt32( visit.Status ) );
                ddlCancelReason.SetValue( visit.CancelReasonValueId );
                ShowCancelReason();

                dtVisitDate.SelectedDateTime = visit.VisitDate;
                nbPagerId.Text = visit.PagerId.HasValue ? visit.PagerId.Value.ToString() : "";
                cbPassportPrinted.Checked = visit.PassportStatus == PassportStatus.Printed;

            }
        }

        /// <summary>
        /// Sets the heading information.
        /// </summary>
        /// <param name="visit">The visit.</param>
        private void SetHeadingInfo( Visit visit )
        {
            hlStatus.Text = visit.Status.ConvertToString();
            switch ( visit.Status )
            {
                case VisitStatus.Active:
                    hlStatus.LabelType = LabelType.Success;
                    break;
                case VisitStatus.Complete:
                    hlStatus.LabelType = LabelType.Info;
                    break;
                case VisitStatus.Cancelled:
                    hlStatus.LabelType = LabelType.Danger;
                    break;
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

        private void BindServicesGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                int? visitId = hfVisitId.Value.AsIntegerOrNull();
                if ( visitId.HasValue )
                {
                    var visit = new VisitService( rockContext ).Get( visitId.Value );

                    gServices.DataSource = visit.Workflows.OrderBy( w => w.WorkflowType.Order ).ToList();
                    gServices.DataBind();
                    gServices.Visible = true;
                }
                else
                {
                    gServices.Visible = false;
                }
            }
        }

        private void BindWorkflowsGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                int? visitId = hfVisitId.Value.AsIntegerOrNull();
                if ( visitId.HasValue )
                {
                    var assessment = new AssessmentService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a => a.VisitId.HasValue && a.VisitId.Value == visitId.Value )
                        .FirstOrDefault();

                    if ( assessment != null )
                    {
                        gWorkflows.DataSource = assessment.Workflows.OrderBy( w => w.WorkflowType.Order ).ToList();
                        gWorkflows.DataBind();
                        gWorkflows.Visible = true;
                    }
                    else
                    {
                        gWorkflows.Visible = false;
                    }
                }
                else
                {
                    gWorkflows.Visible = false;
                }
            }
        }

        /// <summary>
        /// Shows the cancel reason.
        /// </summary>
        private void ShowCancelReason()
        {
            ddlCancelReason.Visible = ddlStatus.SelectedValueAsEnum<VisitStatus>() == VisitStatus.Cancelled;
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "CANCEL":
                    dlgCancel.Show();
                    break;
                case "CANCELWORKFLOW":
                    dlgCancelWorkflow.Show();
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
                case "CANCEL":
                    dlgCancel.Hide();
                    break;
                case "CANCELWORKFLOW":
                    dlgCancelWorkflow.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion


    }
}