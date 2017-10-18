using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 11, "1.5.0" )]
    class AddResourceDirectory : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( "6028D502-79F4-4A74-9323-525E90F900C7", "Referral Resource", "fa fa-user-md", "", SystemGuid.Category.DEFINEDTYPE_REFERRAL_RESOURCE );

            RockMigrationHelper.AddDefinedType( "Referral Resource", "Client Age Ranges", "", "4684F639-A52D-4567-9499-3D304988DC63" );
            RockMigrationHelper.AddDefinedType( "Referral Resource", "Client Genders", "", "BDBACFD2-273A-4B76-87A2-199C7E0806A6" );
            RockMigrationHelper.AddDefinedType( "Referral Resource", "Therapist Genders", "", "859FEC11-4778-4C0B-BC3C-4BE872121E2C" );
            RockMigrationHelper.AddDefinedType( "Referral Resource", "Expertise Keywords", "", "27D85CE3-2BC6-4475-887B-A68A0F14F368" );
            RockMigrationHelper.AddDefinedType( "Referral Resource", "Modality", "", "48A741FD-E257-49A3-9139-7090DD8EA4B7" );
            RockMigrationHelper.AddDefinedType( "Referral Resource", "Keywords", "", "F53DE40E-31AA-4C19-8A9A-9923A7553E19" );
            RockMigrationHelper.AddDefinedType( "Referral Resource", "Insurance Type", "", "0AB32A44-7789-4574-BEC1-F6B376D3001C" );
            RockMigrationHelper.AddDefinedTypeAttribute( "0AB32A44-7789-4574-BEC1-F6B376D3001C", Rock.SystemGuid.FieldType.BOOLEAN, "Adult", "Adult", "", 0, "False", "ADF835AC-17C0-4CFA-BF4E-E03D667D08A7" );
            RockMigrationHelper.AddDefinedTypeAttribute( "0AB32A44-7789-4574-BEC1-F6B376D3001C", Rock.SystemGuid.FieldType.BOOLEAN, "Child", "Child", "", 1, "False", "4F7BFFBF-9ECE-4F42-8596-EE242ACCEFF2" );
            RockMigrationHelper.AddDefinedTypeAttribute( "0AB32A44-7789-4574-BEC1-F6B376D3001C", Rock.SystemGuid.FieldType.BOOLEAN, "Dental", "Dental", "", 2, "False", "F230EFE4-48B9-4571-ABAD-0442090C9985" );
            RockMigrationHelper.AddDefinedTypeAttribute( "0AB32A44-7789-4574-BEC1-F6B376D3001C", Rock.SystemGuid.FieldType.BOOLEAN, "Medical", "Medical", "", 3, "False", "57012201-4EBE-4304-BA19-1B1E2AB64FC9" );
            RockMigrationHelper.AddDefinedTypeAttribute( "0AB32A44-7789-4574-BEC1-F6B376D3001C", Rock.SystemGuid.FieldType.BOOLEAN, "Vision", "Vision", "", 4, "False", "F50ABAB6-461A-4FA4-8D74-3C289E33196E" );
            RockMigrationHelper.AddDefinedTypeAttribute( "0AB32A44-7789-4574-BEC1-F6B376D3001C", Rock.SystemGuid.FieldType.BOOLEAN, "CRN Insurance", "CRNInsurance", "", 5, "False", "72FFB0FD-4257-47A2-93A5-D46D168D6CFD" );

            Sql( @"
CREATE PROC [dbo].[_org_willowcreek_CareCenter_TempInsuranceUpdate] @ForeignGuid uniqueidentifier, @Value nvarchar(max), @Adult bit, @Child bit, @Dental bit, @Medical bit, @Vision bit, @CRN bit 
AS
BEGIN
	DECLARE @AdultAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'ADF835AC-17C0-4CFA-BF4E-E03D667D08A7' )
	DECLARE @ChildAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '4F7BFFBF-9ECE-4F42-8596-EE242ACCEFF2' )
	DECLARE @DentalAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'F230EFE4-48B9-4571-ABAD-0442090C9985' )
	DECLARE @MedicalAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '57012201-4EBE-4304-BA19-1B1E2AB64FC9' )
	DECLARE @VisionAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'F50ABAB6-461A-4FA4-8D74-3C289E33196E' )
	DECLARE @CRNAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '72FFB0FD-4257-47A2-93A5-D46D168D6CFD' )

	DECLARE @DefinedValueId INT
	DECLARE @DefinedTypeId INT = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '0AB32A44-7789-4574-BEC1-F6B376D3001C' )
	DECLARE @Order INT = ISNULL( ( SELECT MAX([Order]) + 1 FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId ), 0 )

	INSERT INTO [DefinedValue] ( [IsSystem], [DefinedTypeId], [Order], [Value], [Guid], [ForeignGuid] )
	VALUES ( 0, @DefinedTypeId, @Order, @Value, NEWID(), @ForeignGuid )
	SET @DefinedValueId = SCOPE_IDENTITY()

	INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId], [Value], [Guid] )
	VALUES ( 0, @AdultAttributeId, @DefinedValueId, CASE WHEN @Adult = 1 THEN 'True' ELSE 'False' END, NEWID() )

	INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId], [Value], [Guid] )
	VALUES ( 0, @ChildAttributeId, @DefinedValueId, CASE WHEN @Child = 1 THEN 'True' ELSE 'False' END, NEWID() )

	INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId], [Value], [Guid] )
	VALUES ( 0, @DentalAttributeId, @DefinedValueId, CASE WHEN @Dental = 1 THEN 'True' ELSE 'False' END, NEWID() )

	INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId], [Value], [Guid] )
	VALUES ( 0, @MedicalAttributeId, @DefinedValueId, CASE WHEN @Medical = 1 THEN 'True' ELSE 'False' END, NEWID() )

	INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId], [Value], [Guid] )
	VALUES ( 0, @VisionAttributeId, @DefinedValueId, CASE WHEN @Vision = 1 THEN 'True' ELSE 'False' END, NEWID() )

	INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId], [Value], [Guid] )
	VALUES ( 0, @CRNAttributeId, @DefinedValueId, CASE WHEN @CRN = 1 THEN 'True' ELSE 'False' END, NEWID() )
END
" );

            Sql( @"
CREATE PROC [dbo].[_org_willowcreek_CareCenter_TempResourcePropertyAdd] @DefinedTypeGuid uniqueidentifier, @ResourceForeignGuid uniqueidentifier, @DefinedValueForeignGuid uniqueidentifier 
AS
BEGIN

	DECLARE @ResourceId INT = ( SELECT TOP 1 [Id] FROM [_org_willowcreek_CareCenter_Resource] WHERE [ForeignGuid] = @ResourceForeignGuid )
	
	DECLARE @DefinedValueId INT = ( 
		SELECT TOP 1 DV.[Id] 
		FROM [DefinedValue] DV INNER JOIN [Definedtype] DT ON DT.[Id] = DV.[DefinedTypeId]
		WHERE DT.[Guid] = @DefinedTypeGuid
		AND DV.[ForeignGuid] = @DefinedValueForeignGuid )

	IF @ResourceId IS NOT NULL AND @DefinedValueId IS NOT NULL
	BEGIN
		INSERT INTO [_org_willowcreek_CareCenter_ResourceProperty] ( [ResourceId],[DefinedValueId],[Guid] )
		VALUES ( @ResourceId, @DefinedValueId, NEWID() )
	END
END
" );
            Sql( CareCenterMigrationsSQL._011_AddResourceDirectory );

            // Page: Resources
            RockMigrationHelper.UpdateBlockType( "Resources", "Care Center block for viewing list of resources.", "~/Plugins/org_willowcreek/CareCenter/Resources.ascx", "org_willowcreek > CareCenter", "903650DA-BE2E-48E2-950D-B34208C72EEB" );
            RockMigrationHelper.AddBlock( "E3B2D931-4D31-459B-87AB-265FE194D7E0", "", "903650DA-BE2E-48E2-950D-B34208C72EEB", "Resources", "Main", "", "", 0, "AB6DB8AA-98B5-4F22-B08E-6D5F1DA55ADB" );
            RockMigrationHelper.AddBlockTypeAttribute( "903650DA-BE2E-48E2-950D-B34208C72EEB", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Main Search Properties", "MainSearchProperties", "", "Custom properties that should always be visible to search on.", 0, @"", "5E3186A1-A639-4445-B779-16677E44BCBC" );
            RockMigrationHelper.AddBlockTypeAttribute( "903650DA-BE2E-48E2-950D-B34208C72EEB", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Filter Search Properties", "FilterSearchProperties", "", "Custom properties that should be visible in the dropdown filter area to search on.", 1, @"", "16381243-C3F6-4EB5-B4F6-1E3D38E89DBA" );
            RockMigrationHelper.AddBlockTypeAttribute( "903650DA-BE2E-48E2-950D-B34208C72EEB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Result Format", "ResultFormat", "", "Lava to use when displaying results", 2, @"
", "A1AB22E0-9110-4AC8-84CC-5A27AB34CDAF" );
            RockMigrationHelper.AddBlockTypeAttribute( "903650DA-BE2E-48E2-950D-B34208C72EEB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page for displaying details of a resource", 3, @"", "64952697-6F9F-4361-BD8C-C5943097321D" );
            RockMigrationHelper.AddBlockAttributeValue( "AB6DB8AA-98B5-4F22-B08E-6D5F1DA55ADB", "64952697-6F9F-4361-BD8C-C5943097321D", @"d0d300f6-8def-47fe-8c47-1879cf403e4a" ); // Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "AB6DB8AA-98B5-4F22-B08E-6D5F1DA55ADB", "5E3186A1-A639-4445-B779-16677E44BCBC", @"97" ); // Main Search Properties
            RockMigrationHelper.AddBlockAttributeValue( "AB6DB8AA-98B5-4F22-B08E-6D5F1DA55ADB", "16381243-C3F6-4EB5-B4F6-1E3D38E89DBA", @"92,93,94,95,96,98" ); // Filter Search Properties
            RockMigrationHelper.AddBlockAttributeValue( "AB6DB8AA-98B5-4F22-B08E-6D5F1DA55ADB", "A1AB22E0-9110-4AC8-84CC-5A27AB34CDAF", @"<div class='panel panel - default'>
    <div class='panel-heading'>
        <strong>{{ Resource.Name }}</strong>
        <span class='pull-right'><a href = '{{ Resource.DetailPageUrl }}' target='resourceDetails'>Details</a></span>
    </div>
    <div class='panel-body'>
        <div class='row'>
            <div class='col-md-12'>
                {{ Resource.Description }}
            </div>
        </div>
        <div class='row'>
            <div class='col-md-6'>
                <div><a href = '{{ Resource.Website }}' target='_blank'>{{ Resource.Website }}</a></div>
                <div>{{ Resource.HtmlAddress }}</div>
                <div>{{ Resource.FirstName }} {{ Resource.LastName }}</div>
                <div>{{ Resource.EmailAddress }}</div>
            </div>
            <div class='col-md-6'>
            </div>
        </div>
    </div>
</div>
"); // Result Format

            // Page: Resource Details
            RockMigrationHelper.AddPage( "E3B2D931-4D31-459B-87AB-265FE194D7E0", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Resource Details", "", "D0D300F6-8DEF-47FE-8C47-1879CF403E4A", "" ); // Site:Care Center
            RockMigrationHelper.AddBlock( "D0D300F6-8DEF-47FE-8C47-1879CF403E4A", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Resource Details", "Main", "", "", 0, "2204B20A-8CCD-4633-A2E7-883FF3DED48B" );
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" ); // Cache Duration
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" ); // Require Approval
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" ); // Enable Versioning
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" ); // Context Parameter
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" ); // Context Name
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" ); // Use Code Editor
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" ); // Image Root Folder
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" ); // User Specific Folders
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" ); // Document Root Folder
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" ); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "8FA9086E-D01A-4D78-99D9-FDB84530BC38", @"RockEntity" ); // Enabled Lava Commands
            RockMigrationHelper.AddBlockAttributeValue( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", "1391ED2D-C918-4040-B400-8748BA3A07EA", @"True" ); // Enable Double-Click Edit

            RockMigrationHelper.UpdateHtmlContentBlock( "2204B20A-8CCD-4633-A2E7-883FF3DED48B", @"
{% resource id:'{{ PageParameter.ResourceId }}' %}
    <h1>{{ resource.Name }}</h1>
{% endresource %}", "80519849-39D6-4B0D-BBDE-1EEE7BC3CD05" );
        }

        public override void Down()
        {
        }
    }
}
