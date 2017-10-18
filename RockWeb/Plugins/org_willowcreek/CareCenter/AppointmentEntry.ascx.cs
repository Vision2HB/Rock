using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Humanizer;

using org.willowcreek.CareCenter;
using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing/editing details of a visit.
    /// </summary>
    [DisplayName( "Appointment Entry" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for adding new appointments." )]

    [TextField( "Service Area Caption", "Caption to use for 'Service Area' selection", false, "Service Area", "", 0)]
    [LinkedPage("Home Page", "Page to return to after appointment is scheduled", false, "", "", 1)]
    [LinkedPage( "Print Page", "Page to use for printing appointment details.", false, "", "", 2 )]
    [CategoryField( "Schedule Exclusion Category", "A Category to check for schedule exclusions.", false, "Rock.Model.Schedule", "", "", false, "", "", 3 )]
    public partial class AppointmentEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        private DateTime? _selectedWeek = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _selectedWeek = ViewState["SelectedWeek"] as DateTime?;
            Guid? serviceAreaGuid = ViewState["ServiceAreaGuid"] as Guid?;
            BuildWeekViewControls( _selectedWeek, serviceAreaGuid );
        }

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

            ddlServiceArea.Label = GetAttributeValue( "ServiceAreaCaption" );
            lConfirmServiceArea.Label = ddlServiceArea.Label;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                LoadDropdowns();

                tglView.Checked = true;

                ShowDetail();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["SelectedWeek"] = _selectedWeek;
            ViewState["ServiceAreaGuid"] = ddlServiceArea.SelectedValueAsGuid();

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var date = hfDay.Value.AsDateTime() ?? RockDateTime.Today;
        }

        /// <summary>
        /// Handles the Click event of the lbPrevDay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrevDay_Click( object sender, EventArgs e )
        {
            var date = hfDay.Value.AsDateTime() ?? RockDateTime.Today;
            ShowDayDetail( date.AddDays( -1 ) );
        }

        /// <summary>
        /// Handles the Click event of the lbNextDay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNextDay_Click( object sender, EventArgs e )
        {
            var date = hfDay.Value.AsDateTime() ?? RockDateTime.Today;
            ShowDayDetail( date.AddDays( 1 ) );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglView_CheckedChanged( object sender, EventArgs e )
        {
            ShowDetail();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlServiceArea control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlServiceArea_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowDetail();
        }

        /// <summary>
        /// Handles the Click event of the lbNextAvailable control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNextAvailable_Click( object sender, EventArgs e )
        {
            lHeading.Text = "Next Available";
            lSubHeading.Text = string.Empty;

            var startTime = RockDateTime.Now;
            var endTime = RockDateTime.Today.AddDays( 180 );

            var timeSlots = GetTimeSlots( startTime, endTime, ddlServiceArea.SelectedValueAsGuid() );

            rptNextAvailable.DataSource = timeSlots.Where( i => i.SlotsRemaining > 0 ).Take( 20 ).ToList();
            rptNextAvailable.DataBind();

            rptTimeSlot.Visible = false;
            nbNoAppointments.Visible = false;
            lbNextAvailable.Visible = false;
            rptNextAvailable.Visible = true;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptTimeSlot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptTimeSlot_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var timeSlotInfo = e.Item.DataItem as TimeSlotInfo;
            if ( timeSlotInfo != null )
            {
                var lblTime = e.Item.FindControl( "lblTime" ) as Label;
                if ( lblTime != null )
                {
                    lblTime.Text = timeSlotInfo.ApptTime.ToString( "h:mm tt" );
                    lblTime.AddCssClass( timeSlotInfo.SlotsRemaining > 0 ? "text-success" : "text-danger" );
                }

                var lblDateTime = e.Item.FindControl( "lblDateTime" ) as Label;
                if ( lblDateTime != null )
                {
                    lblDateTime.Text = timeSlotInfo.ApptTime.Date == RockDateTime.Today ?
                        "Today @ " + timeSlotInfo.ApptTime.ToString( "h:mm tt" ):
                        timeSlotInfo.ApptTime.ToString("dddd MMM d @ h:mm tt" );
                }

                var lblTimeSlotDetails = e.Item.FindControl( "lblTimeSlotDetails" ) as Label;
                if ( lblTimeSlotDetails != null )
                {
                    lblTimeSlotDetails.Text = string.Format( "There are {0} appointments allowed with {1} appointments available.",
                        timeSlotInfo.TimeSlot.RegistrationLimit > 0 ? timeSlotInfo.TimeSlot.RegistrationLimit.ToString( "N0" ) : "no",
                        timeSlotInfo.SlotsRemaining > 0 ? timeSlotInfo.SlotsRemaining.ToString( "N0" ) : "no" );
                }

                var lbScheduleAppt = e.Item.FindControl( "lbScheduleAppt" ) as LinkButton;
                if ( lbScheduleAppt != null )
                {
                    lbScheduleAppt.Visible = timeSlotInfo.SlotsRemaining > 0;
                }
            }
        }

        /// <summary>
        /// Handles the Command event of the lbScheduleAppt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        protected void lbScheduleAppt_Command( object sender, CommandEventArgs e )
        {
            var parts = e.CommandArgument.ToString().SplitDelimitedValues();
            if ( parts.Length == 2 )
            {
                DateTime? date = parts[0].AsDateTime();
                int? timeSlotId = parts[1].AsIntegerOrNull();
                if ( date.HasValue && timeSlotId.HasValue )
                {
                    ShowConfirmation( timeSlotId.Value, date.Value, "Day" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbWeekNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbWeekNext_Click( object sender, EventArgs e )
        {
            ShowWeekDetail( ( _selectedWeek ?? FirstWeekDay( RockDateTime.Today ) ).AddDays( 14 ) );
        }

        /// <summary>
        /// Handles the Click event of the lbWeekPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbWeekPrev_Click( object sender, EventArgs e )
        {
            ShowWeekDetail( ( _selectedWeek ?? FirstWeekDay( RockDateTime.Today) ).AddDays( -14 ) );
        }

        /// <summary>
        /// Handles the Click event of the lbConfirmSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConfirmSchedule_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( PageParameter( "PersonId" ).AsInteger() );
                if ( person == null )
                {
                    ShowError( "Missing Person", "Could not determine who this appointment should be scheduled for. Please try your search again." );
                    return;
                }

                var timeSlot = new ServiceAreaAppointmentTimeslotService( rockContext ).Get( hfConfirmTimeSlot.ValueAsInt() );
                if ( timeSlot == null || timeSlot.ServiceArea == null || timeSlot.ServiceArea.WorkflowType == null || timeSlot.Schedule == null )
                {
                    ShowError( "Missing Appointment Information", "The timeslot for this appointment could not be determined, or the timeslot is not associated with a valid service area and workflow type. Please select the timeslot again." );
                    return;
                }

                var appointmentDate = hfConfirmDate.Value.AsDateTime() ?? RockDateTime.Today;

                var appointmentService = new WorkflowAppointmentService( rockContext );

                var existingAppointments = appointmentService
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.TimeSlotId == timeSlot.Id &&
                        a.AppointmentDate == appointmentDate &&
                        a.Status == AppointmentStatus.Active )
                    .Count();

                if ( existingAppointments >= timeSlot.ScheduleLimit )
                {
                    ShowError( "Appointment Unavailable", "The selected appointment time is no longer available. Please select a different appointment time." );
                    lbConfirmSchedule.Visible = false;
                    lbConfirmCancel.Text = "Return";
                    return;
                }

                var appointment = new WorkflowAppointment();
                appointment.TimeSlotId = timeSlot.Id;
                appointment.PersonAliasId = person.PrimaryAliasId ?? 0;
                appointment.AppointmentDate = appointmentDate;
                appointment.AppointmentType = AppointmentType.Regular;
                appointment.Status = AppointmentStatus.Active;

                Workflow workflow = null;
                WorkflowActivityTypeCache activityType = null;
                var workflowType = WorkflowTypeCache.Read( timeSlot.ServiceArea.WorkflowTypeId.Value );
                if ( workflowType != null )
                {
                    activityType = workflowType.ActivityTypes.FirstOrDefault( a => a.Name == "Appointment" );
                    
                    var workflowName = string.Format( "{0} Appointment: {1}", timeSlot.ServiceArea.Name, person.FullName );
                    workflow = Workflow.Activate( workflowType, person.FullName, rockContext );
                    if ( workflow != null )
                    {
                        workflow.SetAttributeValue( "Person", person.PrimaryAlias.Guid.ToString() );
                        var family = person.GetFamily( rockContext );
                        if ( family != null )
                        {
                            workflow.SetAttributeValue( "Family", family.Guid.ToString() );
                        }

                        DateTime dateTime = appointment.AppointmentDate.Add( timeSlot.Schedule.StartTimeOfDay );
                        workflow.SetAttributeValue( "AppointmentDateTime", dateTime.ToString("o") );

                        bool sendAnnouncement = false;
                        if ( timeSlot.Notification != null )
                        {
                            sendAnnouncement = timeSlot.Notification.SendAnnouncement;
                        }

                        workflow.SetAttributeValue( "SendAnnouncement", sendAnnouncement.ToString() );

                        workflow.Status = "Pending";
                        workflow.Guid = Guid.NewGuid();
                        appointment.Workflow = workflow;
                    }
                }

                appointmentService.Add( appointment );
                rockContext.SaveChanges();

                if ( workflow != null )
                {
                    if ( activityType != null )
                    {
                        WorkflowActivity.Activate( activityType, workflow );
                        rockContext.SaveChanges();
                    }

                    List<string> workflowErrors;
                    new WorkflowService( rockContext ).Process( workflow, out workflowErrors );
                }

                string confirmationSentTo = string.Empty;
                // Send Notification
                try
                {
                    if ( timeSlot.Notification != null && timeSlot.Notification.SendAnnouncement )
                    {
                        appointment = appointmentService.Get( appointment.Id );
                        if ( appointment != null && appointment.PersonAlias != null && appointment.PersonAlias.Person != null && 
                            !string.IsNullOrWhiteSpace( appointment.PersonAlias.Person.Email ) )
                        {
                            // Create the merge fields
                            var mergeFields = new Dictionary<string, object>();
                            mergeFields.Add( "Person", appointment.PersonAlias.Person );
                            mergeFields.Add( "Appointment", appointment );

                            // Merge the email fields
                            confirmationSentTo = appointment.PersonAlias.Person.Email;
                            string subject = timeSlot.Notification.AnnouncementSubject.ResolveMergeFields( mergeFields );
                            string message = timeSlot.Notification.AnnouncementMessage.ResolveMergeFields( mergeFields );

                            // Send the email
                            Email.Send( timeSlot.Notification.FromEmail, timeSlot.Notification.FromName, subject, new List<string> { confirmationSentTo }, message );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
                }

                string confirmationMsg = !string.IsNullOrWhiteSpace( confirmationSentTo ) ? string.Format( "<p>A confirmation has been emailed to <strong>{0}</strong>.</p>", confirmationSentTo ) : "";
                nbSuccess.Text = string.Format( "<p>{0} has been scheduled for a <strong>{1}</strong> appointment for <strong>{2}</strong> at <strong>{3}</strong>.</p>{4}",
                    person.FullName, timeSlot.ServiceArea.Name, appointment.AppointmentDate.ToLongDateString(), timeSlot.DailyTitle, confirmationMsg );


                ShowSuccess( appointment.Id );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbConfirmCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConfirmCancel_Click( object sender, EventArgs e )
        {
            if ( hfPreviousView.Value == "Week")
            {
                ShowWeekDetail( _selectedWeek ?? RockDateTime.Now );
            }
            else
            {
                ShowDayDetail( hfDay.Value.AsDateTime() ?? RockDateTime.Now );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbHome control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHome_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "HomePage" );
        }

        /// <summary>
        /// Handles the Click event of the lbPrint control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrint_Click( object sender, EventArgs e )
        {
        }

        #endregion

        #region Methods

        private void LoadDropdowns()
        {
            ddlServiceArea.Items.Clear();

            int? serviceAreaId = PageParameter( "ServiceAreaId" ).AsIntegerOrNull();

            using ( var rockContext = new RockContext() )
            {
                foreach( var serviceArea in new ServiceAreaService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a => a.AppointmentTimeSlots
                        .Where( t => 
                            t.IsActive == true &&
                            t.Schedule != null && 
                            t.ServiceArea != null &&
                            t.ServiceArea.WorkflowType != null )
                        .Any() )
                    .OrderBy( a => a.Name ) )
                {
                    if ( serviceArea.IsAuthorized( "MakeAppointment", CurrentPerson ) )
                    {
                        ListItem li = new ListItem( serviceArea.Name, serviceArea.Guid.ToString() );
                        li.Selected = serviceAreaId.HasValue && serviceArea.Id == serviceAreaId.Value;
                        ddlServiceArea.Items.Add( li );
                    }
                }
            }
        }

        private void ShowDetail()
        {
            using ( var rockContext = new RockContext() )
            {
                int personId = PageParameter( "PersonId" ).AsInteger();
                var person = new PersonService( rockContext ).Get( personId );
                if ( person != null )
                {
                    lPhoto.Text = string.Format( "<div class=\"photo-round photo-round-sm pull-left\" data-original=\"{0}&w=120\" style=\"background-image: url('{1}');\"></div>", person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-male.svg" ) );
                    lPrimaryContact.Text = person.FullName;

                    Guid? serviceAreaGuid = ddlServiceArea.SelectedValueAsGuid();
                    if ( serviceAreaGuid.HasValue )
                    {
                        DateTime? banExpirationDate = person.GetBannedDateForServiceArea( serviceAreaGuid.Value );
                        if ( banExpirationDate.HasValue && banExpirationDate.Value > RockDateTime.Today )
                        {
                            nbWarningMessage.Heading = "User Banned";
                            nbWarningMessage.Text = string.Format( "<p>This person has been banned from the selected service area until {0} and cannot schedule any appointments before then.", banExpirationDate.Value.ToShortDateString() );
                            nbWarningMessage.Visible = true;
                        }
                        else
                        {
                            nbWarningMessage.Visible = false;
                        }

                        if ( tglView.Checked )
                        {
                            ShowDayDetail( hfDay.Value.AsDateTime() ?? RockDateTime.Today );
                        }
                        else
                        {
                            if ( !_selectedWeek.HasValue )
                            {
                                _selectedWeek = FirstWeekDay( RockDateTime.Today );
                            }

                            ShowWeekDetail( _selectedWeek.Value );
                        }
                    }
                }
            }
        }

        private void ShowDayDetail( DateTime date )
        {
            pnlSearch.Visible = true;
            phDaySelection.Visible = true;
            pnlDayView.Visible = true;
            pnlWeekView.Visible = false;
            pnlConfirm.Visible = false;
            pnlSuccess.Visible = false;

            lHeading.Text = date.DayOfWeek.ConvertToString();
            lSubHeading.Text = date.ToString( "MMMM" ) + " " + date.Day.Ordinalize();

            lbPrevDay.Enabled = date.Date > RockDateTime.Today;
            hfDay.Value = date.ToShortDateString();
            tbDay.Text = FormatDayDisplay( date );

            var timeSlots = GetTimeSlots( date, date.AddDays( 1 ), ddlServiceArea.SelectedValueAsGuid() );
            if ( timeSlots.Any() )
            {
                rptTimeSlot.DataSource = timeSlots;
                rptTimeSlot.DataBind();
                rptTimeSlot.Visible = true;
                nbNoAppointments.Visible = false;
            }
            else
            {
                rptTimeSlot.Visible = false;
                nbNoAppointments.Visible = true;
            }

            lbNextAvailable.Visible = true;
            rptNextAvailable.Visible = false;
        }

        private void ShowWeekDetail( DateTime week )
        {
            pnlSearch.Visible = true;
            phDaySelection.Visible = false;
            pnlDayView.Visible = false;
            pnlWeekView.Visible = true;
            pnlConfirm.Visible = false;
            pnlSuccess.Visible = false;

            DateTime end = week.AddDays( 13 );
            lHeading.Text = string.Format( "{0} - {1}",
                week.ToString( "MMMM " ) + week.Day.Ordinalize(),
                end.ToString( "MMMM " ) + end.Day.Ordinalize() );
            lSubHeading.Text = string.Empty;

            lbWeekPrev.Enabled = week > RockDateTime.Today;

            BuildWeekViewControls( week, ddlServiceArea.SelectedValueAsGuid() );
        }

        private void BuildWeekViewControls( DateTime? week, Guid? serviceAreaGuid )
        {
            if ( week.HasValue )
            {
                _selectedWeek = week.Value;

                phWeek1.Controls.Clear();
                phWeek2.Controls.Clear();

                var weekTimeSlots = GetTimeSlots( week.Value, week.Value.AddDays( 14 ), serviceAreaGuid );

                for ( int i = 0; i < 14; i++ )
                {
                    var day = week.Value.AddDays( i );
                    var nextDay = day.AddDays( 1 );

                    var dayTimeSlots = weekTimeSlots
                        .Where( t =>
                            t.ApptTime >= day &&
                            t.ApptTime < nextDay )
                        .ToList();

                    BuildWeekDayControls( dayTimeSlots, i < 7 ? phWeek1 : phWeek2, day );
                }
            }
        }

        private void BuildWeekDayControls( List<TimeSlotInfo> timeSlots, PlaceHolder ph, DateTime day )
        {
            var td = new HtmlGenericControl( "td" );
            ph.Controls.Add( td );

            var today = RockDateTime.Today;

            if ( day < today )
            {
                td.AddCssClass( "active" );
            }

            else if ( day == RockDateTime.Today )
            {
                td.AddCssClass( "info" );
            }

            td.Controls.Add( new LiteralControl( day.Day.ToString() ) );

            foreach ( var timeSlotInfo in timeSlots)
            {
                td.Controls.Add( new LiteralControl( "<br/>" ) );

                LinkButton lb = new LinkButton();
                td.Controls.Add( lb );
                lb.ID = timeSlotInfo.ControlId;
                lb.Text = string.Format( "{0} <strong>{1}/{2}</strong>", timeSlotInfo.TimeSlot.ScheduleTitle, timeSlotInfo.SlotsTaken, timeSlotInfo.TimeSlot.ScheduleLimit ); 
                lb.Click += Lb_Click;
                lb.Style.Add( "width", "100%" );
                lb.AddCssClass( "btn margin-b-sm input-width-sm" );

                if ( timeSlotInfo.SlotsRemaining > 0 )
                {
                    lb.AddCssClass( "btn-success" );
                }
                else
                {
                    lb.Enabled = false;
                }
            }
        }

        private void Lb_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                var idParts = lb.ID.Split( new char[] { '_' } );
                if ( idParts.Length == 5 )
                {
                    int? timeSlotId = idParts[1].AsIntegerOrNull();
                    DateTime date = new DateTime( idParts[4].AsInteger(), idParts[2].AsInteger(), idParts[3].AsInteger() );
                    if ( timeSlotId.HasValue )
                    {
                        ShowConfirmation( timeSlotId.Value, date, "Week" );
                    }
                }
            }
        }

        private List<TimeSlotInfo> GetTimeSlots( DateTime startTime, DateTime endTime, Guid? serviceAreaGuid )
        {
            var timeSlotList = new List<TimeSlotInfo>();

            using ( var rockContext = new RockContext() )
            {
                int personId = PageParameter( "PersonId" ).AsInteger();
                var person = new PersonService( rockContext ).Get( personId );

                if ( person != null && serviceAreaGuid.HasValue )
                {
                    var now = RockDateTime.Now;

                    var timeSlots = new ServiceAreaAppointmentTimeslotService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( t =>
                            t.ServiceArea != null &&
                            t.ServiceArea.Guid == serviceAreaGuid.Value &&
                            t.IsActive == true &&
                            t.Schedule != null &&
                            t.ServiceArea != null &&
                            t.ServiceArea.WorkflowType != null )
                        .ToList();

                    var timeSlotIds = timeSlots.Select( t => t.Id ).ToList();

                    var existingAppointments = new WorkflowAppointmentService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            timeSlotIds.Contains( a.TimeSlotId ) &&
                            a.AppointmentDate >= startTime.Date &&
                            //a.AppointmentDate >= now &&
                            a.AppointmentDate < endTime &&
                            a.Status == AppointmentStatus.Active )
                        .ToList();

                    DateTime? banExpirationDate = person.GetBannedDateForServiceArea( serviceAreaGuid.Value );

                    var exclusions = new List<ScheduleCategoryExclusion>();
                    var categoryGuid = GetAttributeValue( "ScheduleExclusionCategory" ).AsGuidOrNull();
                    if ( categoryGuid.HasValue )
                    {
                        var category = CategoryCache.Read( categoryGuid.Value );
                        if ( category != null )
                        {
                            exclusions = new ScheduleCategoryExclusionService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( e => e.CategoryId == category.Id )
                                .ToList();
                        }
                    }

                    var start = startTime;
                    while ( start < endTime )
                    {
                        if ( !exclusions.Any( e => e.StartDate <= start && e.EndDate >= start ) &&
                            ( !banExpirationDate.HasValue || start >= banExpirationDate.Value ) )
                        {
                            var end = start.Date.AddDays( 1 );

                            foreach ( var timeSlot in timeSlots )
                            {
                                var startTimes = timeSlot.Schedule.GetScheduledStartTimes( start, end );
                                if ( startTimes.Any() )
                                {
                                    var existingAppts = existingAppointments
                                        .Where( a =>
                                            a.TimeSlotId == timeSlot.Id &&
                                            a.AppointmentDate == start.Date &&
                                            a.Status == AppointmentStatus.Active )
                                        .Count();

                                    foreach ( var timeSlotStart in startTimes )
                                    {
                                        if ( timeSlotStart >= start &&
                                            timeSlotStart >= now &&
                                            !timeSlotList.Any( t => t.ApptTime == timeSlotStart && t.TimeSlot.Id == timeSlot.Id ) )
                                        {
                                            var timeSlotInfo = new TimeSlotInfo( timeSlotStart, timeSlot, existingAppts );
                                            timeSlotInfo.SlotsTaken = existingAppts;
                                            timeSlotList.Add( timeSlotInfo );
                                        }
                                    }
                                }
                            }
                        }

                        start = start.Date.AddDays( 1 );
                    }
                }
            }

            return timeSlotList.OrderBy( i => i.ApptTime ).ToList();
        }


        private void ShowConfirmation( int timeSlotId, DateTime date, string prevView )
        {
            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( PageParameter( "PersonId" ).AsInteger() );
                if ( person == null )
                {
                    ShowError( "Missing Person", "Could not determine who this appointment should be scheduled for. Please try your search again." );
                    return;
                }

                var timeSlot = new ServiceAreaAppointmentTimeslotService( rockContext ).Get( timeSlotId );
                if ( timeSlot == null || timeSlot.ServiceArea == null || timeSlot.ServiceArea.WorkflowType == null )
                {
                    ShowError( "Missing Appointment Information", "The timeslot for this appointment could not be determined, or the timeslot is not associated with a valid service area and workflow type. Please select the timeslot again." );
                    return;
                }

                if ( !timeSlot.ServiceArea.IsAuthorized("MakeAppointment", CurrentPerson))
                {
                    ShowError( "Security Error", "You are not currently authorized to make appointments for the selected Service Area." );
                    return;
                }

                pnlSearch.Visible = false;
                pnlConfirm.Visible = true;
                pnlSuccess.Visible = false;

                lConfirmPerson.Text = person.FullName;
                lConfirmServiceArea.Text = timeSlot.ServiceArea != null ? timeSlot.ServiceArea.Name : "";
                lConfirmAppointment.Text = date.ToString( "MMMM" ) + " " + date.Day.Ordinalize() + " - " + timeSlot.ScheduleTitle;

                hfPreviousView.Value = prevView;
                hfConfirmTimeSlot.Value = timeSlot.Id.ToString();
                hfConfirmDate.Value = date.ToShortDateString();

                lbConfirmSchedule.Visible = true;
                lbConfirmCancel.Text = "Cancel";
            }
        }

        private void ShowSuccess( int apptId )
        {
            hlPrint.NavigateUrl = LinkedPageUrl( "PrintPage", new Dictionary<string, string> { { "AppointmentId", apptId.ToString() } } );
            pnlSearch.Visible = false;
            pnlConfirm.Visible = false;
            pnlSuccess.Visible = true;
        }

        private string FormatDayDisplay( DateTime date )
        {
            var today = RockDateTime.Today;
            if ( date.Date == today )
            {
                return "Today";
            }

            if ( date.Date == today.AddDays( 1 ) )
            {
                return "Tomorrow";
            }

            return date.ToShortDateString();
        }

        private void ShowError( string title, string message )
        {
            nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
            nbMessage.Title = title;
            nbMessage.Text = message;
            nbMessage.Visible = true;
        }

        private DateTime FirstWeekDay( DateTime? dateTime )
        {
            var date = dateTime.HasValue ? dateTime.Value.Date : RockDateTime.Today;
            var sundayDate = date.SundayDate( DayOfWeek.Sunday );
            return sundayDate > date ? sundayDate.AddDays( -7 ) : sundayDate;
        }

        #endregion

        #region Helper Class

        private class TimeSlotInfo
        {
            public DateTime ApptTime { get; set; }
            public ServiceAreaAppointmentTimeslot TimeSlot { get; set; }
            public int SlotsTaken { get; set; }

            public int SlotsRemaining
            {
                get
                {
                    if ( TimeSlot != null && TimeSlot.ScheduleLimit > SlotsTaken )
                    {
                        return TimeSlot.ScheduleLimit - SlotsTaken;
                    }
                    return 0;
                }
            }

            public string ControlId
            {
                get
                {
                    return string.Format( "lb_{0}_{1}", TimeSlot.Id, ApptTime.ToString("MM_dd_yyyy") );
                }
            }
      
            public TimeSlotInfo( DateTime apptTime, ServiceAreaAppointmentTimeslot timeSlot, int slotsTaken )
            {
                ApptTime = apptTime;
                TimeSlot = timeSlot;
                SlotsTaken = slotsTaken;
            }
        }

        #endregion

    }

}