using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Resource Property Service
    /// </summary>
    public partial class ResourcePropertyService
    {

        /// <summary>
        /// Gets the by resource identifier.
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns></returns>
        public IQueryable<ResourceProperty> GetByResourceId( int resourceId )
        {
            return Queryable( "DefinedValue" ).Where( p =>
                p.ResourceId == resourceId );
        }

        public List<ResourcePropertyHelper> GetPropertySummary( int resourceId )
        {
            var categoryGuid = org.willowcreek.CareCenter.SystemGuid.Category.DEFINEDTYPE_REFERRAL_RESOURCE.AsGuid();
            var propertyTypes = new DefinedTypeService( (RockContext)this.Context )
                .Queryable().AsNoTracking()
                .Where( t =>
                    t.Category != null &&
                    t.Category.Guid.Equals( categoryGuid ) )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .Select( t => new ResourcePropertyHelper
                {
                    Id = t.Id,
                    PropertyType = t.Name,
                    Properties = ""
                } )
                .ToList();

            var properties = GetByResourceId( resourceId )
                .AsNoTracking()
                .ToList();

            foreach ( var propertyType in propertyTypes )
            {
                propertyType.Properties = properties
                    .Where( p => p.DefinedValue.DefinedTypeId == propertyType.Id )
                    .OrderBy( p => p.DefinedValue.Id )
                    .Select( p => p.DefinedValue.Value )
                    .ToList()
                    .AsDelimited( ", " );
            }

            return propertyTypes;
        }

    }
}
