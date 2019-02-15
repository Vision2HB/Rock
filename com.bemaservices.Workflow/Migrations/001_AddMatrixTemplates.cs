using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.Workflow.Migrations
{
    [MigrationNumber( 1, "1.8.0" )]
    class AddMatrixTemplates : Migration
    {
        public override void Up()
        {
            throw new Exception();
            //// Setup Attribute Matrix: Survey Questions
            //Sql( @"INSERT INTO [dbo].[AttributeMatrixTemplate] (
            //    [Name]
            //    ,[Description]
            //    ,[IsActive]
            //    ,[FormattedLava]
            //    ,[Guid]
            //)
            //VALUES (
            //    'Survey Questions'
            //    ,'A list of question responses to person attributes.'
            //    ,1
            //    ,'{% if AttributeMatrixItems != empty %}  <table class=''grid-table table table-condensed table-light''> <thead> <tr> {% for itemAttribute in ItemAttributes %}     <th>{{ itemAttribute.Name }}</th> {% endfor %} </tr> </thead> <tbody> {% for attributeMatrixItem in AttributeMatrixItems %} <tr>     {% for itemAttribute in ItemAttributes %}         <td>{{ attributeMatrixItem | Attribute:itemAttribute.Key }}</td>     {% endfor %} </tr> {% endfor %} </tbody> </table>  {% endif %}'
            //    ,'C9619E4A-051D-4ED0-913E-B5274808055A'
            //)

            //DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = TRY_CAST('C9619E4A-051D-4ED0-913E-B5274808055A' AS uniqueidentifier));
            
            //-- It's possible that the attribute matrix entity type query below will fail if this plugin is installed on a brand new environment during testing.          
            //DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
            //DECLARE @IntegerFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('A75DFC58-7A1B-4799-BF31-451B2BBE38FF' AS uniqueidentifier));
            //DECLARE @TextFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('9C204CD0-1233-41C5-818A-C5DA439445AA' AS uniqueidentifier));
            //DECLARE @AttributeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('99B090AA-4D7E-46D8-B393-BF945EA1BA8B' AS uniqueidentifier));

            //INSERT INTO [dbo].[Attribute] (
	           //  [IsSystem]
	           // ,[FieldTypeId]
	           // ,[EntityTypeId]
	           // ,[EntityTypeQualifierColumn]
	           // ,[EntityTypeQualifierValue]
	           // ,[Key]
	           // ,[Name]
	           // ,[Description]
	           // ,[Order]
	           // ,[IsGridColumn]
	           // ,[IsMultiValue]
	           // ,[IsRequired]
	           // ,[Guid]
	           // ,[AllowSearch]
	           // ,[IsIndexEnabled]
	           // ,[IsAnalytic]
	           // ,[IsAnalyticHistory]
            //)
            //VALUES (0, @IntegerFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', @AttributeMatrixTemplateId, 'QuestionId', 'Question Id', 'The question id from Survey Gizmo.', 0, 0, 0, 1, 'c893e725-4639-4158-a8b5-3e142910296a', 0, 0, 0, 0 ),
            //(0, @TextFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', @AttributeMatrixTemplateId, 'Description1', 'Description', '', 1, 0, 0, 1, '48a37873-03ad-401d-93fa-ae6467801af4', 0, 0, 0, 0 ),
            //(0, @AttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', @AttributeMatrixTemplateId, 'PersonAttribute', 'Person Attribute', 'The person attribute that will store the result of the question response.', 2, 0, 0, 1, '8072cdd0-3dd7-4e5f-b364-a7ba3df64d6d', 0, 0, 0, 0 )

            //DECLARE @PersonAttributeAttributeId INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = TRY_CAST('8072cdd0-3dd7-4e5f-b364-a7ba3df64d6d' AS uniqueidentifier));

            //-- Attribute Qualifier: entitytype
            //INSERT INTO [dbo].[AttributeQualifier] (
	           //  [IsSystem]
	           // ,[AttributeId]
	           // ,[Key]
	           // ,[Value]
	           // ,[Guid]
            //)
            //VALUES (
	           //  0
	           // ,@PersonAttributeAttributeId
	           // ,'entitytype'
	           // ,'72657ed8-d16e-492e-ac12-144c5e7567e7'
	           // ,'56df8c2e-b6ee-4920-8603-1de7c8045a01'
            //)" );

        }

        public override void Down()
        {
            
        }
    }
}
