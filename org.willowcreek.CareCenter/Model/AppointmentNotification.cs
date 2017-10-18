using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Care Center Service Area
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_AppointmentNotification" )]
    [DataContract]
    public class AppointmentNotification : Model<AppointmentNotification>, IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the email from.
        /// </summary>
        /// <value>
        /// The name of the email from.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the email from email.
        /// </summary>
        /// <value>
        /// The email from email.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send reminders].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [send reminders]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public Boolean SendReminders { get; set; }

        /// <summary>
        /// Gets or sets the send reminder days ahead.
        /// </summary>
        /// <value>
        /// The send reminder days ahead.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string SendReminderDaysAhead { get; set; }

        /// <summary>
        /// Gets or sets the reminder subject.
        /// </summary>
        /// <value>
        /// The reminder subject.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string ReminderSubject { get; set; }

        /// <summary>
        /// Gets or sets the reminder message.
        /// </summary>
        /// <value>
        /// The reminder message.
        /// </value>
        [DataMember]
        public string ReminderMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send announcement].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [send announcement]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public Boolean SendAnnouncement { get; set; }

        /// <summary>
        /// Gets or sets the announcement subject.
        /// </summary>
        /// <value>
        /// The announcement subject.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string AnnouncementSubject { get; set; }

        /// <summary>
        /// Gets or sets the announcement message.
        /// </summary>
        /// <value>
        /// The announcement message.
        /// </value>
        [DataMember]
        public string AnnouncementMessage { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    public partial class AppointmentNotificationConfiguration : EntityTypeConfiguration<AppointmentNotification>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentNotificationConfiguration"/> class.
        /// </summary>
        public AppointmentNotificationConfiguration()
        {
            this.HasEntitySetName( "AppointmentNotifications" );
        }
    }
}
