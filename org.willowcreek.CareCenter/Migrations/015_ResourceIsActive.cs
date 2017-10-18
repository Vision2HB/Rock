using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 15, "1.5.0" )]
    class ResourceIsActive : Migration
    {
        public override void Up()
        {
            Sql( @"
    ALTER TABLE [_org_willowcreek_CareCenter_Resource] ADD 
        [IsActive] bit NULL;
" );

            Sql( @"
    UPDATE [_org_willowcreek_CareCenter_Resource]
    SET [IsActive] = 1
" );
        }

        public override void Down()
        {
        }
    }
}
