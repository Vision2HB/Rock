using System;
using System.ComponentModel;
using System.Web.UI;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    [DisplayName( "Appointment Notification Detail" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Displays the details of the given appointment notification." )]
    public partial class AppointmentNotificationDetail : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "NotificationId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            AppointmentNotification notification;
            using ( var rockContext = new RockContext() )
            {
                AppointmentNotificationService appointmentNotificationService = new AppointmentNotificationService( rockContext );
                int appointmentNotificationId = hfAppointmentNotificationId.Value.AsInteger();
                if ( appointmentNotificationId == 0 )
                {
                    notification = new AppointmentNotification();
                    appointmentNotificationService.Add( notification );
                }
                else
                {
                    notification = appointmentNotificationService.Get( appointmentNotificationId );
                }

                notification.Name = tbName.Text;
                notification.FromName = tbFromName.Text;
                notification.FromEmail = tbFromEmail.Text;
                notification.SendReminders = cbSendReminders.Checked;
                notification.SendReminderDaysAhead = tbDaysAhead.Text;
                notification.ReminderSubject = tbReminderSubject.Text;
                notification.ReminderMessage = ceReminderBody.Text;
                notification.SendAnnouncement = cbSendAnnouncement.Checked;
                notification.AnnouncementSubject = tbAnnouncementSubject.Text;
                notification.AnnouncementMessage = ceAnnouncementBody.Text;
                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="appointmentNotificationId">The Appointment Notification identifier.</param>
        public void ShowDetail( int appointmentNotificationId )
        {
            AppointmentNotification notification = null;
            bool editAllowed = UserCanEdit;

            using ( var rockContext = new RockContext() )
            {
                if ( !appointmentNotificationId.Equals( 0 ) )
                {
                    notification = new AppointmentNotificationService( rockContext ).Get( appointmentNotificationId );
                }

                if ( notification == null )
                {
                    notification = new AppointmentNotification { Id = 0 };
                    pdAuditDetails.Visible = false;

                    lActionTitle.Text = ActionTitle.Add( AppointmentNotification.FriendlyTypeName ).FormatAsHtmlTitle();
                }
                else
                {
                    pdAuditDetails.SetEntity( notification, ResolveRockUrl( "~" ) );
                    lActionTitle.Text = notification.Name.FormatAsHtmlTitle();
                }

                hfAppointmentNotificationId.Value = notification.Id.ToString();

                if ( UserCanEdit )
                {
                    nbEditModeMessage.Text = string.Empty;
                    pnlEditDetails.Visible = true;

                    tbName.Text = notification.Name;
                    tbFromName.Text = notification.FromName;
                    tbFromEmail.Text = notification.FromEmail;
                    cbSendReminders.Checked = notification.SendReminders;
                    tbDaysAhead.Text = notification.SendReminderDaysAhead;
                    tbReminderSubject.Text = notification.ReminderSubject;
                    ceReminderBody.Text = notification.ReminderMessage;
                    cbSendAnnouncement.Checked = notification.SendAnnouncement;
                    tbAnnouncementSubject.Text = notification.AnnouncementSubject;
                    ceAnnouncementBody.Text = notification.AnnouncementMessage;
                }
                else
                {
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Person.FriendlyTypeName );
                    pnlEditDetails.Visible = false;
                }
            }
        }

        #endregion
    }
}