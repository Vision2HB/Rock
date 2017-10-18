using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock;
using Rock.Data;
using Rock.Model;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Care Center Service Area
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_ServiceArea" )]
    [DataContract]
    public class ServiceArea : Model<ServiceArea>, IRockEntity, IOrdered
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
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the passport lava.
        /// </summary>
        /// <value>
        /// The passport lava.
        /// </value>
        [DataMember]
        public string PassportLava { get; set; }

        /// <summary>
        /// Gets or sets the intake lava.
        /// </summary>
        /// <value>
        /// The intake lava.
        /// </value>
        [DataMember]
        public string IntakeLava { get; set; }

        /// <summary>
        /// Gets or sets the workflow type identifier.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        [DataMember]
        public int? WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [workflow allows scheduling].
        /// </summary>
        /// <value>
        /// <c>true</c> if [workflow allows scheduling]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool WorkflowAllowsScheduling { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [uses passport].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [uses passport]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool UsesPassport { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        [LavaInclude]
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [LavaInclude]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has schedule.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has schedule; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasSchedule
        {
            get { return this.AppointmentTimeSlots != null && this.AppointmentTimeSlots.Any(); }
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [LavaInclude]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the service area bans.
        /// </summary>
        /// <value>
        /// The service area bans.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ServiceAreaAppointmentTimeslot> AppointmentTimeSlots
        {
            get { return _timeSlots; }
            set { _timeSlots = value; }
        }
        private ICollection<ServiceAreaAppointmentTimeslot> _timeSlots;

        /// <summary>
        /// Gets or sets the service area bans.
        /// </summary>
        /// <value>
        /// The service area bans.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ServiceAreaBan> ServiceAreaBans
        {
            get { return _serviceAreaBans; }
            set { _serviceAreaBans = value; }
        }
        private ICollection<ServiceAreaBan> _serviceAreaBans;

        #endregion

        #region Methods

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var actions = base.SupportedActions;
                actions.AddOrReplace( "MakeAppointment", "The user or roles that are authorized to schedule appointments for this service area." );
                return actions;
            }
        }

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

        public ServiceArea()
        {
            _serviceAreaBans = new Collection<ServiceAreaBan>();
            _timeSlots = new Collection<ServiceAreaAppointmentTimeslot>();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public partial class ServiceAreaConfiguration : EntityTypeConfiguration<ServiceArea>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAreaConfiguration"/> class.
        /// </summary>
        public ServiceAreaConfiguration()
        {
            this.HasOptional( a => a.Category ).WithMany().HasForeignKey( a => a.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Schedule ).WithMany().HasForeignKey( a => a.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.WorkflowType ).WithMany().HasForeignKey( a => a.WorkflowTypeId ).WillCascadeOnDelete( false );
            this.HasEntitySetName( "ServiceAreas" );
        }
    }
}
