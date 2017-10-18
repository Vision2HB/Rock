using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Care Center Service Area Ban
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_ServiceAreaBan" )]
    [DataContract]
    public class ServiceAreaBan : Model<ServiceAreaBan>, IRockEntity
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
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }


        /// <summary>
        /// Gets or sets the ban expire date.
        /// </summary>
        /// <value>
        /// The ban expire date.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime BanExpireDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the service area.
        /// </summary>
        /// <value>
        /// The service area.
        /// </value>
        [LavaInclude]
        public virtual ServiceArea ServiceArea { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }
        #endregion

        #region Methods

        #endregion
    }


    /// <summary>
    /// 
    /// </summary>
    public partial class ServiceAreaBanConfiguration : EntityTypeConfiguration<ServiceAreaBan>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAreaBanConfiguration"/> class.
        /// </summary>
        public ServiceAreaBanConfiguration()
        {
            this.HasRequired( a => a.ServiceArea ).WithMany( s => s.ServiceAreaBans ).HasForeignKey( a => a.ServiceAreaId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasEntitySetName( "ServiceAreaBans" );
        }
    }
}
