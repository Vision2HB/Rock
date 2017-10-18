using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 10, "1.5.0" )]
    class AddJobs : Migration
    {
        public override void Up()
        {
            Sql( @"
     INSERT INTO [ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid] )
    VALUES 
    (
         0
        ,1
        ,'Care Center: Close Open Workflows (Afternoon)'
        ,'Cancels any care center visit workflows that are still open.'
        ,'org.willowcreek.CareCenter.Jobs.CloseServiceAreas'
        ,'0 0 14 1/1 * ? *'
        ,3
        ,'10DCFB9E-2163-43C3-BD1C-89034E39BDBB'),
    (
         0
        ,1
        ,'Care Center: Close Open Workflows (Evening)'
        ,'Cancels any care center visit workflows that are still open.'
        ,'org.willowcreek.CareCenter.Jobs.CloseServiceAreas'
        ,'0 0 19 1/1 * ? *'
        ,3
        ,'0EABE4C5-6BBF-4140-85B2-4805DF4C270F'),
    (
         0
        ,1
        ,'Care Center: Send Appointment Reminders'
        ,'Sends appointment reminders.'
        ,'org.willowcreek.CareCenter.Jobs.SendAppointmentReminders'
        ,'0 0 3 1/1 * ? *'
        ,3
        ,'36F8F581-6DD3-42E9-8D68-5CD8EB9DC500')
" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Class", "org.willowcreek.CareCenter.Jobs.CloseServiceAreas", "Workflows", "The workflow types to close if not yet completed.", 0, "", "26B750D1-6D98-4A46-989A-719EB70B2DA6" );
            RockMigrationHelper.UpdateAttributeQualifier( "26B750D1-6D98-4A46-989A-719EB70B2DA6", "values", @"
        SELECT 
	        CAST(W.[Guid] AS VARCHAR(50)) AS [Value], 
	        C.[Name] + '': '' + W.[Name] AS [Text] 
        FROM [WorkflowType] W
        INNER JOIN [Category] C ON C.[Id] = W.[CategoryId]
        WHERE C.[Guid] IN ( ''11601938-4335-41FA-9D44-04CA1054649E'', ''C068535D-F1AD-430A-9F65-57E228A3CF86'' )
        ORDER BY C.[Name], W.[Name]
", "6BA1746E-6E93-4CCE-8BF3-49807ADF12AC" );

            Sql( @"
	    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '26B750D1-6D98-4A46-989A-719EB70B2DA6' )

        DECLARE @JobId int = ( SELECT TOP 1 [Id] FROM [ServiceJob] WHERE [Guid] = '10DCFB9E-2163-43C3-BD1C-89034E39BDBB' )
        IF @JobId IS NOT NULL AND @AttributeId IS NOT NULL
	    BEGIN
            IF NOT EXISTS ( SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @JobId )
            BEGIN
		        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		        VALUES ( 0, @AttributeId, @JobId, '2A5ACB4A-69DE-4D16-BDA3-C7187297D3B9,99830D9E-E8AF-4757-A84E-2D4C4EC5BE09,9196C4D9-A820-4217-9341-44B2C0084F74,2A76CDB2-F330-4C9B-A1E8-24308041CB03,BCE8C80C-81FF-4580-9000-A09A4B8920E3,988FDF54-86D4-4CC3-BB4F-DE25A9D494B2,6B37C278-5C83-49DF-86B7-3F4C51AB52CD,46B04B7C-699C-4513-804E-BA54A11B2AF9,EA47030D-D539-4509-BA46-A57ED61D61E4', NEWID() )
            END
	    END

        SET @JobId = ( SELECT TOP 1 [Id] FROM [ServiceJob] WHERE [Guid] = '0EABE4C5-6BBF-4140-85B2-4805DF4C270F' )
        IF @JobId IS NOT NULL AND @AttributeId IS NOT NULL
	    BEGIN
            IF NOT EXISTS ( SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @JobId )
            BEGIN
		        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		        VALUES ( 0, @AttributeId, @JobId, '2A5ACB4A-69DE-4D16-BDA3-C7187297D3B9,99830D9E-E8AF-4757-A84E-2D4C4EC5BE09,9196C4D9-A820-4217-9341-44B2C0084F74,2A76CDB2-F330-4C9B-A1E8-24308041CB03,BCE8C80C-81FF-4580-9000-A09A4B8920E3,988FDF54-86D4-4CC3-BB4F-DE25A9D494B2,6B37C278-5C83-49DF-86B7-3F4C51AB52CD,46B04B7C-699C-4513-804E-BA54A11B2AF9,EA47030D-D539-4509-BA46-A57ED61D61E4', NEWID() )
            END
	    END

" );
        }

        public override void Down()
        {
        }
    }
}
