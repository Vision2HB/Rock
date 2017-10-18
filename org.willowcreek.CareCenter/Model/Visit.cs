using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Care Center Visit
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_Visit" )]
    [DataContract]
    public class Visit : Model<Visit>, IRockEntity
    {
        #region Entity Properties

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
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public VisitStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the cancel reason value identifier.
        /// </summary>
        /// <value>
        /// The cancel reason value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( org.willowcreek.CareCenter.SystemGuid.DefinedType.VISIT_CANCEL_REASON)]
        public int? CancelReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the visit date.
        /// </summary>
        /// <value>
        /// The visit date.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime VisitDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether person's photo id has been validated
        /// </summary>
        /// <value>
        /// <c>true</c> if [photo id validated]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool PhotoIdValidated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether person's proof of address has been validated
        /// </summary>
        /// <value>
        /// <c>true</c> if [proof of address validated]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool ProofOfAddressValidated { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public bool? IsHomeless { get; set; }

        /// <summary>
        /// Gets or sets the pager identifier.
        /// </summary>
        /// <value>
        /// The pager identifier.
        /// </value>
        [DataMember]
        public int? PagerId { get; set; }

        /// <summary>
        /// Gets or sets the passport status.
        /// </summary>
        /// <value>
        /// The passport status.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public PassportStatus PassportStatus { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        [LavaInclude]
        public virtual DefinedValue CancelReasonValue { get; set; }

        /// <summary>
        /// Gets or sets the workflows.
        /// </summary>
        /// <value>
        /// The workflows.
        /// </value>
        [LavaInclude]
        public virtual ICollection<Rock.Model.Workflow> Workflows
        {
            get { return _workflows; }
            set { _workflows = value; }
        }
        private ICollection<Rock.Model.Workflow> _workflows;

        /// <summary>
        /// Gets a value indicating whether this instance is first visit.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is first visit; otherwise, <c>false</c>.
        /// </value>
        [LavaInclude]
        public virtual bool IsFirstVisit
        {
            get { return this.FirstVisit(); }
        }

        #endregion

        #region Methods

        #endregion

        #region Constructor

        public Visit()
        {
            _workflows = new Collection<Rock.Model.Workflow>();
        }

        #endregion

    }


    /// <summary>
    /// 
    /// </summary>
    public partial class VisitConfiguration : EntityTypeConfiguration<Visit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisitConfiguration"/> class.
        /// </summary>
        public VisitConfiguration()
        {
            this.HasRequired( v => v.PersonAlias ).WithMany().HasForeignKey( v => v.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( v => v.CancelReasonValue).WithMany().HasForeignKey( v => v.CancelReasonValueId ).WillCascadeOnDelete( false );
            this.HasMany( v => v.Workflows ).WithMany().Map( v => { v.MapLeftKey( "VisitId" ); v.MapRightKey( "WorkflowId" ); v.ToTable( "_org_willowcreek_CareCenter_VisitWorkflow" ); } );
            this.HasEntitySetName( "Visits" );
        }
    }


    /// <summary>
    /// Visit Status
    /// </summary>
    public enum VisitStatus
    {
        /// <summary>
        /// Active
        /// </summary>
        Active = 0,

        /// <summary>
        /// Complete
        /// </summary>
        Complete = 1,

        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled = 2,
    }

    /// <summary>
    /// Status of passport for a visit
    /// </summary>
    public enum PassportStatus
    {
        /// <summary>
        /// Not Printed
        /// </summary>
        NotPrinted = 0,

        /// <summary>
        /// Printed
        /// </summary>
        Printed = 1,

        /// <summary>
        /// Passport is not needed
        /// </summary>
        NotNeeded = 2,
    }

}
