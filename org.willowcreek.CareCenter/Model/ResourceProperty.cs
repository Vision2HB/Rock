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
    /// Care Center Resource Property
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_ResourceProperty" )]
    [DataContract]
    public class ResourceProperty : Model<ResourceProperty>, IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        /// <value>
        /// The resource identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the defined value identifier.
        /// </summary>
        /// <value>
        /// The defined value identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int DefinedValueId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>
        /// The resource.
        /// </value>
        [LavaInclude]
        public virtual Resource Resource { get; set; }

        /// <summary>
        /// Gets or sets the defined value.
        /// </summary>
        /// <value>
        /// The defined value.
        /// </value>
        [LavaInclude]
        public virtual DefinedValue DefinedValue { get; set; }

        #endregion

    }


    /// <summary>
    /// 
    /// </summary>
    public partial class ResourcePropertyConfiguration : EntityTypeConfiguration<ResourceProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePropertyConfiguration"/> class.
        /// </summary>
        public ResourcePropertyConfiguration()
        {
            this.HasRequired( p => p.Resource).WithMany( r => r.ResourceProperties).HasForeignKey( p => p.ResourceId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.DefinedValue).WithMany().HasForeignKey( p => p.DefinedValueId ).WillCascadeOnDelete( true );
            this.HasEntitySetName( "ResourceProperties" );
        }
    }
    
}
