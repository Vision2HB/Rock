
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;


namespace com.bemaservices.Workflow.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected value
    /// </summary>
    [ActionCategory( "BEMA Services > Workflow Attributes" )]
    [Description( "Sets an attribute's value to the selected value." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attributes Set Values" )]

    [MatrixField( "7308FDDC-8123-4C22-8DBC-E283F9F12C1D", "Set Workflow Attributes", "", false, "", 0, "Matrix" )]
    public class SetAttributeValue : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var attributeMatrixGuid = GetAttributeValue( action, "Matrix" ).AsGuid();
            var attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid );
            if ( attributeMatrix != null )
            {
                foreach ( AttributeMatrixItem attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
                {
                    attributeMatrixItem.LoadAttributes();

                    var attribute = AttributeCache.Get( attributeMatrixItem.GetAttributeValue( "WorkflowAttribute" ).AsGuid(), rockContext );
                    if ( attribute != null )
                    {
                        string value = value = attributeMatrixItem.GetAttributeValue( "Value" ).ResolveMergeFields( GetMergeFields( action ) );
                        if ( attribute.FieldTypeId == FieldTypeCache.Get( Rock.SystemGuid.FieldType.ENCRYPTED_TEXT.AsGuid(), rockContext ).Id ||
                            attribute.FieldTypeId == FieldTypeCache.Get( Rock.SystemGuid.FieldType.SSN.AsGuid(), rockContext ).Id )
                        {
                            value = Rock.Security.Encryption.EncryptString( value );
                        }

                        SetWorkflowAttributeValue( action, attribute.Guid, value );
                        action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, value ) );
                    }
                }
            }

            return true;
        }

    }
}