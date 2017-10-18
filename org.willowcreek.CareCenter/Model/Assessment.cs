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
    /// Care Center Assessment
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_Assessment" )]
    [DataContract]
    public class Assessment : Model<Assessment>, IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the visit identifier.
        /// </summary>
        /// <value>
        /// The visit identifier.
        /// </value>
        [DataMember]
        public int? VisitId { get; set; }

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
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the assessment date.
        /// </summary>
        /// <value>
        /// The assessment date.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime AssessmentDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the visit.
        /// </summary>
        /// <value>
        /// The visit.
        /// </value>
        [LavaInclude]
        public virtual Visit Visit { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the approved by person alias.
        /// </summary>
        /// <value>
        /// The approved by person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias ApprovedByPersonAlias { get; set; }

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
        /// Gets a value indicating whether this instance is first assessment.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is first assessment; otherwise, <c>false</c>.
        /// </value>
        [LavaInclude]
        public virtual bool IsFirstAssessment
        {
            get { return this.FirstAssessment(); }
        }

        #endregion

        #region Methods

        #endregion

        #region Constructor

        public Assessment()
        {
            _workflows = new Collection<Rock.Model.Workflow>();
        }

        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    public partial class AssessmentConfiguration : EntityTypeConfiguration<Assessment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssessmentConfiguration"/> class.
        /// </summary>
        public AssessmentConfiguration()
        {
            this.HasOptional( v => v.Visit ).WithMany().HasForeignKey( v => v.VisitId ).WillCascadeOnDelete( false );
            this.HasRequired( v => v.PersonAlias ).WithMany().HasForeignKey( v => v.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( v => v.ApprovedByPersonAlias ).WithMany().HasForeignKey( v => v.ApprovedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasMany( v => v.Workflows ).WithMany().Map( v => { v.MapLeftKey( "AssessmentId" ); v.MapRightKey( "WorkflowId" ); v.ToTable( "_org_willowcreek_CareCenter_AssessmentWorkflow" ); } );
            this.HasEntitySetName( "Assessments" );
        }
    }

}
