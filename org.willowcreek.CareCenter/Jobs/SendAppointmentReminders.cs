using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using org.willowcreek.CareCenter.Model;
using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace org.willowcreek.CareCenter.Jobs
{
    /// <summary>
    /// Job to send appointment reminders.
    /// </summary>
    [DisallowConcurrentExecution]
    public class SendAppointmentReminders : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendAppointmentReminders()
        {
        }

        /// <summary>
        /// Job that will close workflows.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                var workflowIds = new List<int>();

                var today = RockDateTime.Today;
                var now = RockDateTime.Now;

                // Get all the appointments that have a future appointment date and who's timelot notification is configured for reminders
                var appointments = new WorkflowAppointmentService( rockContext )
                    .Queryable()
                    .Where( a => 
                        a.AppointmentDate >= today &&
                        a.PersonAlias != null &&
                        a.PersonAlias.Person != null &&
                        a.PersonAlias.Person.Email != null &&
                        a.PersonAlias.Person.Email != "" &&
                        a.TimeSlot != null &&
                        a.TimeSlot.Notification != null &&
                        a.TimeSlot.Notification.SendReminders )
                    .ToList();

                foreach ( var appointment in appointments )
                {
                    var notification = appointment.TimeSlot.Notification;

                    // If the notification is valid
                    if ( !string.IsNullOrWhiteSpace( notification.FromEmail ) &&
                        !string.IsNullOrWhiteSpace( notification.ReminderMessage ) &&
                        appointment.AppointmentDateTime >= now )
                    {
                        // Get the number of days until the appointment.
                        int daysOut = Convert.ToInt32( appointment.AppointmentDate.Subtract( now.Date ).TotalDays );

                        // Get all the configured reminder days that are greater or equal to the number of days until appt.
                        var reminderDays = notification.SendReminderDaysAhead.SplitDelimitedValues().AsIntegerList().Where( i => i >= daysOut ).ToList();

                        // Get the days that reminder has already ben sent for
                        var sentDays = appointment.RemindersSent.SplitDelimitedValues().AsIntegerList();

                        // Find any missing reminder days
                        var unsentDays = reminderDays.Where( d => !sentDays.Contains( d ) ).ToList();

                        // If there are any missing
                        if ( unsentDays.Any() )
                        {
                            // Create the merge fields
                            var mergeFields = new Dictionary<string, object>();
                            mergeFields.Add( "Person", appointment.PersonAlias.Person );
                            mergeFields.Add( "Appointment", appointment );

                            // Merge the email fields
                            string emailAddress = appointment.PersonAlias.Person.Email;
                            string subject = notification.ReminderSubject.ResolveMergeFields( mergeFields );
                            string message = notification.ReminderMessage.ResolveMergeFields( mergeFields );

                            // Send the email
                            Email.Send( notification.FromEmail, notification.FromName, subject, new List<string> { emailAddress }, message );

                            // Update the days sent to include any missed reminder days
                            sentDays.AddRange( unsentDays );
                            appointment.RemindersSent = sentDays.OrderByDescending( d => d ).ToList().AsDelimited( "," );

                            // Save the changes
                            rockContext.SaveChanges();
                        }
                    }
                }

            }

        }

    }
}
