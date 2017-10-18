using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Data;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// User control for managing the Service Area for the Care Center
    /// </summary>
    [DisplayName( "Appointment Time Slot Detail" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "User control for managing the Service Area Time Slot for the Care Center." )]

    public partial class TimeSlotDetail : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "TimeSlotId" ).AsInteger(), PageParameter( "ServiceAreaId" ).AsInteger() );
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
            ServiceAreaAppointmentTimeslot timeslot = null;

            using ( var rockContext = new RockContext() )
            {
                var service = new ServiceAreaAppointmentTimeslotService( rockContext );

                int? timeslotId = PageParameter("TimeSlotId").AsIntegerOrNull();
                if ( timeslotId.HasValue && timeslotId.Value != 0 )
                {
                    timeslot = service.Get( timeslotId.Value );
                }

                if ( timeslot == null )
                {
                    timeslot = new ServiceAreaAppointmentTimeslot();
                    timeslot.ServiceAreaId = PageParameter( "ServiceAreaId" ).AsInteger();
                    service.Add( timeslot );
                }

                if ( timeslot.Schedule == null )
                {
                    timeslot.Schedule = new Rock.Model.Schedule();
                }

                timeslot.ScheduleTitle = tbScheduleTitle.Text;
                timeslot.IsActive = cbIsActive.Checked;
                timeslot.Schedule.iCalendarContent = sbSchedule.iCalendarContent;
                timeslot.RegistrationLimit = nbRegistrationLimit.Text.AsInteger();
                timeslot.WalkupLimit = nbWalkupLimit.Text.AsInteger();
                timeslot.AllowPublicRegistration = cbAllowPublicRegistration.Checked;
                timeslot.DailyTitle = tbDailyTitle.Text;
                timeslot.NotificationId = ddlNotification.SelectedValueAsInt();

                if ( !timeslot.IsValid )
                {
                    nbTimeSlot.Text = string.Format( "Please Correct the Following<ul><li>{0}</li></ul>", timeslot.ValidationResults.AsDelimited( "</li><li>" ) );
                    return;
                }

                if ( !Page.IsValid )
                {
                    return;
                }

                rockContext.SaveChanges();
            }

            NavigateToParentPage( new Dictionary<string, string> { { "ServiceAreaId", PageParameter( "ServiceAreaId" ) } } );
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage( new Dictionary<string, string> { { "ServiceAreaId", PageParameter( "ServiceAreaId" ) } } );
        }

        #endregion

        #region Internal Methods


        private void ShowDetail( int timeSlotId, int serviceAreaId )
        {
            using ( var rockContext = new RockContext() )
            {
                LoadDropdowns( rockContext );

                ServiceArea serviceArea = null;
                ServiceAreaAppointmentTimeslot timeslot = null;

                if ( timeSlotId != 0 )
                {
                    timeslot = new ServiceAreaAppointmentTimeslotService( rockContext ).Get( timeSlotId );
                }
                if ( timeslot == null )
                {
                    timeslot = new ServiceAreaAppointmentTimeslot();
                    timeslot.IsActive = true;
                }
                else
                {
                    serviceArea = timeslot.ServiceArea;
                }

                if ( serviceArea == null )
                {
                    serviceArea = new ServiceAreaService( rockContext ).Get( serviceAreaId );
                }

                if ( serviceArea != null )
                {
                    timeslot.ServiceArea = serviceArea;
                    lActionTitle.Text = string.Format( "{0} Time Slot", serviceArea.Name );
                }

                tbScheduleTitle.Text = timeslot.ScheduleTitle;
                cbIsActive.Checked = timeslot.IsActive;

                if ( timeslot.Schedule != null )
                {
                    sbSchedule.iCalendarContent = timeslot.Schedule.iCalendarContent;
                } 
                else
                {
                    sbSchedule.iCalendarContent = null;
                }

                nbRegistrationLimit.Text = timeslot.RegistrationLimit.ToString();
                nbWalkupLimit.Text = timeslot.WalkupLimit.ToString();
                cbAllowPublicRegistration.Checked = timeslot.AllowPublicRegistration;
                tbDailyTitle.Text = timeslot.DailyTitle;
                ddlNotification.SetValue( timeslot.NotificationId );
            }

        }

        private void LoadDropdowns( RockContext rockContext )
        {
            ddlNotification.Items.Clear();
            ddlNotification.Items.Add( new ListItem() );
            foreach( var notification in new AppointmentNotificationService( rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( n => n.Name ) )
            {
                ddlNotification.Items.Add( new ListItem( notification.Name, notification.Id.ToString() ) );
            }
        }

        #endregion

    }
}