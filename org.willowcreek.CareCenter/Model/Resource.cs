using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Runtime.Serialization;
using DotLiquid;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Care Center Resource
    /// </summary>
    [Table( "_org_willowcreek_CareCenter_Resource" )]
    [DataContract]
    public class Resource : Model<Resource>, IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the owner value identifier.
        /// </summary>
        /// <value>
        /// The owner value identifier.
        /// </value>
        [DefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_OWNER )]
        [DataMember]
        public int? OwnerValueId { get; set; }

        /// <summary>
        /// Gets or sets the type value identifier.
        /// </summary>
        /// <value>
        /// The type value identifier.
        /// </value>
        [DefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE )]
        [DataMember]
        public int? TypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the street.
        /// </summary>
        /// <value>
        /// The street.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the geo point.
        /// </summary>
        /// <value>
        /// The geo point.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography GeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the website.
        /// </summary>
        /// <value>
        /// The website.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets the reduced fee program particpant.
        /// </summary>
        /// <value>
        /// The reduced fee program particpant.
        /// </value>
        [DataMember]
        public bool? ReducedFeeProgramParticpant { get; set; }

        /// <summary>
        /// Gets or sets the support groups offerred.
        /// </summary>
        /// <value>
        /// The support groups offerred.
        /// </value>
        [DataMember]
        public bool? SupportGroupsOfferred { get; set; }

        /// <summary>
        /// Gets or sets the sliding fee offered.
        /// </summary>
        /// <value>
        /// The sliding fee offered.
        /// </value>
        [DataMember]
        public bool? SlidingFeeOffered { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [MaxLength( 300 )]
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [MaxLength( 300 )]
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [MaxLength( 30 )]
        [DataMember]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the mobile number.
        /// </summary>
        /// <value>
        /// The mobile number.
        /// </value>
        [MaxLength( 30 )]
        [DataMember]
        public string MobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the willow attender.
        /// </summary>
        /// <value>
        /// The willow attender.
        /// </value>
        [DataMember]
        public bool? WillowAttender { get; set; }

        /// <summary>
        /// Gets or sets the interview date time.
        /// </summary>
        /// <value>
        /// The interview date time.
        /// </value>
        [DataMember]
        public DateTime? InterviewDateTime { get; set; }

        #endregion

        #region Virtual Properties

        [LavaInclude]
        public virtual DefinedValue OwnerValue { get; set; }

        [LavaInclude]
        public virtual DefinedValue TypeValue { get; set; }

        /// <summary>
        /// Gets or sets the resource properties.
        /// </summary>
        /// <value>
        /// The resource properties.
        /// </value>
        [LavaInclude]
        public virtual ICollection<ResourceProperty> ResourceProperties
        {
            get { return _resourceProperties; }
            set { _resourceProperties = value; }
        }
        private ICollection<ResourceProperty> _resourceProperties;

        [LavaInclude]
        [NotMapped]
        public string Address
        {
            get
            {
                return
                    ( string.IsNullOrWhiteSpace( Street ) ? "" : Street + Environment.NewLine ) +
                    ( string.IsNullOrWhiteSpace( City ) ? "" : City + ", " ) +
                    ( string.IsNullOrWhiteSpace( State ) ? "" : State + " " ) +
                    PostalCode;
            }
        }

        [LavaInclude]
        [NotMapped]
        public string HtmlAddress
        {
            get
            {
                return Address.ConvertCrLfToHtmlBr();
            }
        }

        [LavaInclude]
        [NotMapped]
        public List<ResourcePropertyHelper> PropertyTypes
        {
            get
            {
                var properties = new List<DefinedValueCache>();
                foreach ( var dvId in this.ResourceProperties
                    .Select( p => p.DefinedValueId )
                    .ToList() )
                {
                    properties.Add( DefinedValueCache.Read( dvId ) );
                }

                var propertyTypes = properties
                    .OrderBy( p => p.DefinedType.Order )
                    .ThenBy( p => p.DefinedType.Name )
                    .Select( p => new
                    {
                        PropertyTypeId = p.DefinedType.Id,
                        PropertyType = p.DefinedType.Name
                    } )
                    .Distinct()
                    .Select( p => new ResourcePropertyHelper
                    {
                        Id = p.PropertyTypeId,
                        PropertyType = p.PropertyType,
                        Properties = ""
                    } )
                    .ToList();

                foreach( var propertyType in propertyTypes)
                {
                    propertyType.Properties = properties
                        .Where( p => p.DefinedTypeId == propertyType.Id )
                        .OrderBy( p => p.Order )
                        .Select( p => p.Value )
                        .ToList()
                        .AsDelimited( ", " );
                }

                return propertyTypes;
            }
        }


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

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        public Resource()
        {
            _resourceProperties = new Collection<ResourceProperty>();
            IsActive = true;
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ResourceConfiguration : EntityTypeConfiguration<Resource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceConfiguration"/> class.
        /// </summary>
        public ResourceConfiguration()
        {
            this.HasOptional( r => r.OwnerValue ).WithMany().HasForeignKey( r => r.OwnerValueId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.TypeValue ).WithMany().HasForeignKey( r => r.TypeValueId ).WillCascadeOnDelete( false );

            this.HasEntitySetName( "Resources" );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DotLiquid.LiquidType( "Id", "DateTime", "PropertyType", "Properties" )]
    public class ResourcePropertyHelper
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [LavaInclude]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        [LavaInclude]
        public string PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        [LavaInclude]
        public string Properties { get; set; }
    }
}