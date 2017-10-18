using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Care Center Workflow Appointment
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_WorkflowAppointment" )]
    [DataContract]
    public class WorkflowAppointment : Model<WorkflowAppointment>, IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the timeslot identifier.
        /// </summary>
        /// <value>
        /// The timeslot identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int TimeSlotId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the appointment date.
        /// </summary>
        /// <value>
        /// The appointment date.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// Gets or sets the type of the appointment.
        /// </summary>
        /// <value>
        /// The type of the appointment.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public AppointmentType AppointmentType { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public AppointmentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the days ahead of appointment that reminders have been sent.
        /// </summary>
        /// <value>
        /// The reminders sent.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string RemindersSent { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual Rock.Model.Workflow Workflow { get; set; }

        [LavaInclude]
        public virtual ServiceAreaAppointmentTimeslot TimeSlot { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets the appointment date time.
        /// </summary>
        /// <value>
        /// The appointment date time.
        /// </value>
        [LavaInclude]
        public virtual DateTime AppointmentDateTime
        {
            get
            {
                var date = AppointmentDate.Date;
                if ( TimeSlot != null && TimeSlot.Schedule != null )
                {
                    var startTime = TimeSlot.Schedule.GetScheduledStartTimes( date, date.AddDays( 1 ) ).FirstOrDefault();
                    if ( startTime != null )
                    {
                        return startTime;
                    }
                }
                return date;
            }
        }

        #endregion

        #region Methods

        #endregion

        #region Constructor

        #endregion

    }


    /// <summary>
    /// 
    /// </summary>
    public partial class WorkflowAppointmentConfiguration : EntityTypeConfiguration<WorkflowAppointment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowAppointmentConfiguration"/> class.
        /// </summary>
        public WorkflowAppointmentConfiguration()
        {
            this.HasRequired( a => a.Workflow ).WithMany().HasForeignKey( a => a.WorkflowId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.TimeSlot).WithMany().HasForeignKey( a => a.TimeSlotId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasEntitySetName( "WorkflowAppointments" );
        }
    }

    /// <summary>
    /// Appointment Type
    /// </summary>
    public enum AppointmentType
    {
        /// <summary>
        /// Regular
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Walk-in Appointment
        /// </summary>
        Walkin = 1,

    }

    /// <summary>
    /// Appointment Status
    /// </summary>
    public enum AppointmentStatus
    {
        /// <summary>
        /// Appointment Active
        /// </summary>
        Active = 0,

        /// <summary>
        /// Appointment Cancelled
        /// </summary>
        Cancelled = 1,

        /// <summary>
        /// Appointment No-Show
        /// </summary>
        NoShow = 2,

        /// <summary>
        /// Appointment Arrived
        /// </summary>
        Arrived = 3,

    }
}
