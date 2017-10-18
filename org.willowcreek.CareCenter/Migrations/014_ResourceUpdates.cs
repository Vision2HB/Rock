using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 14, "1.5.0" )]
    class ResourceUpdates : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "", "Referral Resource Owner", "The team responsible for a Referral Resource.", SystemGuid.DefinedType.REFERRAL_RESOURCE_OWNER );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_OWNER, "Pastoral Response", "", "A03F881A-4201-4CD7-B46C-5A34FE8DEDE8" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_OWNER, "Care Center", "", "83298E3B-A7F2-4150-8D04-78F081EE66D3" );

            RockMigrationHelper.AddDefinedType( "", "Referral Resource Type", "The type of resource.", SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE, "Counselor", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE, "External Organization", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE, "Willow Ministry", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE, "Support Group", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE, "Book", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.REFERRAL_RESOURCE_TYPE, "Event", "" );


            Sql( @"
    ALTER TABLE [_org_willowcreek_CareCenter_Resource] ADD 
        [OwnerValueId] int NULL,
        [TypeValueId] int NULL;

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Resource]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Resource_dbo.DefinedValue_OwnerValueId] FOREIGN KEY([OwnerValueId])
    REFERENCES [dbo].[DefinedValue] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Resource] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Resource_dbo.DefinedValue_OwnerValueId]

    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Resource]  WITH CHECK ADD  CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Resource_dbo.DefinedValue_TypeValueId] FOREIGN KEY([TypeValueId])
    REFERENCES [dbo].[DefinedValue] ([Id])
    ALTER TABLE [dbo].[_org_willowcreek_CareCenter_Resource] CHECK CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Resource_dbo.DefinedValue_TypeValueId]
" );

        Sql( @"
    DECLARE @ExpertiseDtId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '27D85CE3-2BC6-4475-887B-A68A0F14F368' )
    DECLARE @PastoralResponseDvId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'A03F881A-4201-4CD7-B46C-5A34FE8DEDE8' )
    DECLARE @CareTeamDvId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '83298E3B-A7F2-4150-8D04-78F081EE66D3' )

    UPDATE [_org_willowcreek_CareCenter_Resource]
    SET [OwnerValueId] = 
        CASE WHEN [Id] IN (
		    SELECT P.[ResourceId]
		    FROM [_org_willowcreek_CareCenter_ResourceProperty] P
		    INNER JOIN [DefinedValue] DV ON DV.[Id] = P.[DefinedValueId]
		    WHERE DV.[DefinedTypeId] = @ExpertiseDtId
	    ) THEN @PastoralResponseDvId ELSE @CareTeamDvId END 

" );
        }

        public override void Down()
        {
        }
    }
}
