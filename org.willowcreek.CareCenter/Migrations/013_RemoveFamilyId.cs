using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 13, "1.5.0" )]
    class RemoveFamilyId : Migration
    {
        public override void Up()
        {
            Sql( @"
    DROP INDEX [IX_FamilyId] ON [_org_willowcreek_CareCenter_Visit]
    ALTER TABLE [_org_willowcreek_CareCenter_Visit] DROP CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Visit_dbo.Group_FamilyId]
    ALTER TABLE [_org_willowcreek_CareCenter_Visit] DROP COLUMN [FamilyId]

    DROP INDEX [IX_FamilyId] ON [_org_willowcreek_CareCenter_Assessment]
    ALTER TABLE [_org_willowcreek_CareCenter_Assessment] DROP CONSTRAINT [FK_dbo._org_willowcreek_CareCenter_Assessment_dbo.Group_FamilyId]
    ALTER TABLE [_org_willowcreek_CareCenter_Assessment] DROP COLUMN [FamilyId]
" );
        }

        public override void Down()
        {
        }
    }
}
