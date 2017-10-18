using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Care Center Service Area
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_ServiceAreaAppointmentTimeslot" )]
    [DataContract]
    public class ServiceAreaAppointmentTimeslot : Model<ServiceAreaAppointmentTimeslot>, IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the service area identifier.
        /// </summary>
        /// <value>
        /// The service area identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ServiceAreaId { get; set; }

        /// <summary>
        /// Gets or sets the notification identifier.
        /// </summary>
        /// <value>
        /// The notification identifier.
        /// </value>
        [DataMember]
        public int? NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string ScheduleTitle { get; set; }

        /// <summary>
        /// Gets or sets the daily title.
        /// </summary>
        /// <value>
        /// The daily title.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string DailyTitle { get; set; }

        /// <summary>
        /// Gets or sets the registration limit.
        /// </summary>
        /// <value>
        /// The registration limit.
        /// </value>
        [DataMember]
        public int RegistrationLimit { get; set; }

        /// <summary>
        /// Gets or sets the walkup limit.
        /// </summary>
        /// <value>
        /// The walkup limit.
        /// </value>
        [DataMember]
        public int WalkupLimit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow public registration].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow public registration]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool AllowPublicRegistration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the number of scheduled registrations that are allowed.
        /// </summary>
        /// <value>
        /// The schedule limit.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public int ScheduleLimit
        {
            get { return RegistrationLimit - WalkupLimit; }
        }

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        [LavaInclude]
        public virtual ServiceArea ServiceArea { get; set; }

        /// <summary>
        /// Gets or sets the notification.
        /// </summary>
        /// <value>
        /// The notification.
        /// </value>
        [LavaInclude]
        public virtual AppointmentNotification Notification { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [LavaInclude]
        public virtual Schedule Schedule { get; set; }

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
            return ScheduleTitle;
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ServiceAreaAppointmentTimeslotConfiguration : EntityTypeConfiguration<ServiceAreaAppointmentTimeslot>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAreaAppointmentTimeslotaConfiguration"/> class.
        /// </summary>
        public ServiceAreaAppointmentTimeslotConfiguration()
        {
            this.HasRequired( a => a.ServiceArea ).WithMany( n => n.AppointmentTimeSlots ).HasForeignKey( a => a.ServiceAreaId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Notification ).WithMany().HasForeignKey( a => a.NotificationId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Schedule ).WithMany().HasForeignKey( a => a.ScheduleId ).WillCascadeOnDelete( false );
            this.HasEntitySetName( "ServiceAreaAppointmentTimeslots" );
        }
    }
}
