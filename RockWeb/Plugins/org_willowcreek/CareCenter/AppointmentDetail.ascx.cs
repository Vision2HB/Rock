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
    /// Care Center block for viewing/editing details of a appointment.
    /// </summary>
    [DisplayName( "Appointment Detail" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing/editing details of a appointment." )]

    [LinkedPage( "Appointment Entry Page", "Page to direct user to when the appointment needs to be rescheduled.", true, "", "", 0 )]
    [LinkedPage( "Print Page", "Page to use for printing appointment details.", false, "", "", 1 )]
    [LinkedPage( "Person Profile Page", "Page used for viewing person.", false, "", "", 2 )]
    public partial class AppointmentDetail : Rock.Web.UI.RockBlock
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

            lbEdit.Visible = UserCanEdit;

            string deleteScript = string.Format( @"
    $('#{0}').click(function( e ){{
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to cancel this appointment?', function (result) {{
            if (result) {{
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }}
        }});
    }});

    $('#{1}').click(function( e ){{
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to mark this appointment as a no show?', function (result) {{
            if (result) {{
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }}
        }});
    }});
", lbCancelAppointment.ClientID, lbNoShow.ClientID );
            ScriptManager.RegisterStartupScript( pnlViewDetails, pnlViewDetails.GetType(), "deleteScript", deleteScript, true );

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
            pnlUpdateMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "AppointmentId" ).AsInteger() );
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
            int appointmentId = hfAppointmentId.ValueAsInt();
            ShowReadonlyDetails( GetAppointment( appointmentId ) );
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetAppointment( hfAppointmentId.Value.AsInteger() ) );
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
                var appointmentService = new WorkflowAppointmentService( rockContext );
                WorkflowAppointment appointment = null;

                int? appointmentId = hfAppointmentId.Value.AsIntegerOrNull();
                if ( appointmentId.HasValue )
                {
                    appointment = appointmentService.Get( appointmentId.Value );
                }

                if ( appointment == null )
                {
                    nbServerValidation.Heading = "Save Error";
                    nbServerValidation.Text = "The appointment record could not be found. It may have been deleted while you were editing the values.";
                    nbServerValidation.Visible = true;
                    return;
                }

                appointment.PersonAliasId = ppPrimaryContact.PersonAliasId ?? 0;
                appointment.Status = ddlStatus.SelectedValueAsEnum<AppointmentStatus>();

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !appointment.IsValid )
                {
                    nbServerValidation.Heading = string.Empty;
                    nbServerValidation.Text = string.Format( "Please correct the following:<ul><li>{0}</li></ul", appointment.ValidationResults.AsDelimited( "</li><li>" ) );
                    nbServerValidation.Visible = true;
                    return;
                }

                rockContext.SaveChanges();

                hfAppointmentId.SetValue( appointment.Id );

                // Requery the batch to support EF navigation properties
                var savedAppointment = GetAppointment( appointment.Id );
                ShowReadonlyDetails( savedAppointment );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int appointmentId = hfAppointmentId.ValueAsInt();
            ShowReadonlyDetails( GetAppointment( appointmentId ) );
        }

        /// <summary>
        /// Handles the Click event of the lbCancelAppointment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelAppointment_Click( object sender, EventArgs e )
        {
            CancelAppointment( AppointmentStatus.Cancelled );
        }

        protected void lbNoShow_Click( object sender, EventArgs e )
        {
            CancelAppointment( AppointmentStatus.NoShow );
        }

        protected void lbStartVisit_Click( object sender, EventArgs e )
        {
            nbPagerNumber.Text = string.Empty;
            ShowDialog( "StartVisit" );
        }

        protected void dlgStartVisit_SaveClick( object sender, EventArgs e )
        {
            StartVisit();
        }

        #endregion

            #region Methods

        private WorkflowAppointment GetAppointment( int appointmentId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var appointment = new WorkflowAppointmentService( rockContext ).Get( appointmentId );
            return appointment;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="appointmentId">The financial appointment identifier.</param>
        public void ShowDetail( int appointmentId )
        {
            ddlStatus.BindToEnum<AppointmentStatus>();

            WorkflowAppointment appointment = GetAppointment( appointmentId );
            if ( appointment == null )
            {
                nbWarningMessage.Text = "Could not find selected Appointment.";
                nbWarningMessage.Visible = true;
                SetEditMode( null );
            }
            else
            {
                hfAppointmentId.Value = appointment.Id.ToString();
                lbEdit.Visible = IsUserAuthorized( Authorization.EDIT );
                ShowReadonlyDetails( appointment );
            }
        }

        /// <summary>
        /// Shows the financial appointment summary.
        /// </summary>
        /// <param name="appointment">The financial appointment.</param>
        private void ShowReadonlyDetails( WorkflowAppointment appointment )
        {
            if ( appointment == null )
            {
                SetEditMode( null );
            }
            else
            {
                SetEditMode( false );

                SetHeadingInfo( appointment );
                lPhoto.Text = string.Format( "<div class=\"photo-round photo-round-sm pull-left\" data-original=\"{0}&w=120\" style=\"background-image: url('{1}');\"></div>", appointment.PersonAlias.Person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-male.svg" ) );

                var dict = new Dictionary<string, string>();

                if ( appointment.PersonAlias != null && appointment.PersonAlias.Person != null )
                {
                    string personName = appointment.PersonAlias.Person.FullName;
                    string personUrl = LinkedPageUrl( "PersonProfilePage", new Dictionary<string, string> { { "PersonId", appointment.PersonAlias.Person.Id.ToString() } } );
                    lPrimaryContact.Text = !String.IsNullOrWhiteSpace( personUrl ) ? string.Format( "<a href='{0}' target='_blank'>{1}</a>", personUrl, personName ) : personName;

                    foreach( var phoneNumber in appointment.PersonAlias.Person.PhoneNumbers )
                    {
                        if ( phoneNumber.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )
                        { 
                            lMobilePhone.Text = phoneNumber.NumberFormatted;
                        }
                        else if ( phoneNumber.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() )
                        {
                            lHomePhone.Text = phoneNumber.NumberFormatted;
                        }
                    }

                    dict.Add( "PersonId", appointment.PersonAlias.PersonId.ToString() );
                }

                lDate.Text = appointment.AppointmentDate.ToLongDateString();

                if ( appointment.TimeSlot != null && appointment.TimeSlot.ServiceArea != null )
                {
                    lServiceArea.Text = appointment.TimeSlot.ServiceArea.Name;
                    lTimeSlot.Text = appointment.TimeSlot.ScheduleTitle;

                    dict.Add( "ServiceAreaId", appointment.TimeSlot.ServiceAreaId.ToString() );
                }

                hlPrint.NavigateUrl = LinkedPageUrl( "PrintPage", new Dictionary<string, string> { { "AppointmentId", appointment.Id.ToString() } } );

                lbCancelAppointment.Visible = appointment.Status == AppointmentStatus.Active;
                lbNoShow.Visible = appointment.Status == AppointmentStatus.Active;
                lbStartVisit.Visible = appointment.Status == AppointmentStatus.Active;

                hlReschedule.NavigateUrl = LinkedPageUrl( "AppointmentEntryPage", dict );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="appointment">The appointment.</param>
        protected void ShowEditDetails( WorkflowAppointment appointment )
        {
            if ( appointment == null )
            {
                SetEditMode( null );
            }
            else
            {
                SetEditMode( true );

                ppPrimaryContact.PersonId = appointment.PersonAlias.PersonId;
                ppPrimaryContact.PersonName = appointment.PersonAlias.Person.FullName;

                ddlStatus.SetValue( Convert.ToInt32( appointment.Status ) );
            }
        }

        /// <summary>
        /// Sets the heading information.
        /// </summary>
        /// <param name="appointment">The appointment.</param>
        private void SetHeadingInfo( WorkflowAppointment appointment )
        {
            hlStatus.Text = appointment.Status.ConvertToString();
            switch ( appointment.Status )
            {
                case AppointmentStatus.Active:
                    hlStatus.LabelType = LabelType.Success;
                    break;
                case AppointmentStatus.NoShow:
                    hlStatus.LabelType = LabelType.Danger;
                    break;
                case AppointmentStatus.Cancelled:
                    hlStatus.LabelType = LabelType.Warning;
                    break;
                case AppointmentStatus.Arrived:
                    hlStatus.LabelType = LabelType.Success;
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

        private void CancelAppointment( AppointmentStatus status )
        {
            int? appointmentId = hfAppointmentId.Value.AsIntegerOrNull();

            using ( var rockContext = new RockContext() )
            {
                var appointmentService = new WorkflowAppointmentService( rockContext );
                if ( appointmentId.HasValue )
                {
                    var appointment = appointmentService.Get( appointmentId.Value );
                    if ( appointment != null )
                    {
                        var now = RockDateTime.Now;
                        if ( appointment.Workflow != null )
                        {
                            if ( status == AppointmentStatus.Cancelled )
                            {
                                appointment.Workflow.Status = "Cancel";
                            }

                            if ( status == AppointmentStatus.NoShow )
                            {
                                appointment.Workflow.Status = "NoShow";
                            }

                            List<string> workflowErrors;
                            new Rock.Model.WorkflowService( rockContext ).Process( appointment.Workflow, out workflowErrors );
                        }

                        appointment.Status = status;

                        rockContext.SaveChanges();

                        if ( status == AppointmentStatus.Cancelled )
                        {
                            lblUpdateMessage.Text = "Appointment Has Been Cancelled!";
                        }

                        if ( status == AppointmentStatus.NoShow )
                        {
                            lblUpdateMessage.Text = "Appointment Has Been Marked as a No-Show!";
                        }

                        pnlUpdateMessage.Visible = true;
                        hlReschedule.Visible = true;
                    }
                }
            }

            ShowDetail( appointmentId ?? 0 );
        }

        private void StartVisit()
        {
            int? appointmentId = hfAppointmentId.Value.AsIntegerOrNull();

            using ( var rockContext = new RockContext() )
            {
                var appointmentService = new WorkflowAppointmentService( rockContext );
                if ( appointmentId.HasValue )
                {
                    var appointment = appointmentService.Get( appointmentId.Value );
                    if ( appointment != null )
                    {
                        var visitService = new VisitService( rockContext );

                        var Visit = new Visit();
                        Visit.VisitDate = RockDateTime.Now;
                        Visit.PassportStatus = PassportStatus.NotPrinted;
                        visitService.Add( Visit );

                        Visit.PersonAliasId = appointment.PersonAliasId;
                        Visit.Status = VisitStatus.Active;
                        Visit.CancelReasonValueId = null;
                        Visit.PagerId = nbPagerNumber.Text.AsIntegerOrNull();

                        appointment.Workflow.Status = "Waiting";
                        Visit.Workflows.Add( appointment.Workflow );

                        appointment.Status = AppointmentStatus.Arrived;

                        rockContext.SaveChanges();

                        List<string> workflowErrors;
                        new Rock.Model.WorkflowService( rockContext ).Process( appointment.Workflow, out workflowErrors );

                        lblUpdateMessage.Text = "Visit has been started!";

                        pnlUpdateMessage.Visible = true;
                        hlReschedule.Visible = false;
                    }
                }
            }

            HideDialog();
            ShowDetail( appointmentId ?? 0 );
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
                case "STARTVISIT":
                    dlgStartVisit.Show();
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
                case "STARTVISIT":
                    dlgStartVisit.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion


    }
}