using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Resource Property Service
    /// </summary>
    public partial class ResourceService
    {

        /// <summary>
        /// Gets the by resource identifier.
        /// </summary>
        /// <param name="value">The attribute value.</param>
        /// <returns></returns>
        public bool SaveProvidedResources( int workflowId, string value )
        {
            var rockContext = (RockContext)this.Context;

            // Get the workflow entitytype and the Response Pastor workflow type
            var workflowEntityType = EntityTypeCache.Read( "Rock.Model.Workflow" );
            int? workflowTypeId = new WorkflowService( rockContext )
                .Queryable().AsNoTracking()
                .Where( w => w.Id == workflowId )
                .Select( w => w.WorkflowTypeId )
                .FirstOrDefault();

            if ( workflowEntityType != null && workflowTypeId.HasValue )
            {
                string qualifierValue = workflowTypeId.Value.ToString();

                var attribute = new AttributeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.EntityTypeId == workflowEntityType.Id &&
                        a.EntityTypeQualifierColumn == "WorkflowTypeId" &&
                        a.EntityTypeQualifierValue == qualifierValue &&
                        a.Key == "ResourcesProvided" )
                    .FirstOrDefault();

                if ( attribute != null )
                {
                    var attributeValueService = new AttributeValueService( rockContext );
                    var attributeValue = attributeValueService.Queryable()
                        .Where( v =>
                            v.AttributeId == attribute.Id &&
                            v.EntityId == workflowId )
                        .FirstOrDefault();
                    if ( attributeValue == null )
                    {
                        attributeValue = new AttributeValue();
                        attributeValueService.Add( attributeValue );
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.EntityId = workflowId;
                    }

                    attributeValue.Value = value;

                    return true;
                }
            }

            return false;
        }

    }
}
