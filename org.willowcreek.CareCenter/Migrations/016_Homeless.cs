using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 16, "1.5.0" )]
    class Homeless : Migration
    {
        public override void Up()
        {
            Sql( @"
    ALTER TABLE [_org_willowcreek_CareCenter_Visit] ADD 
        [IsHomeless] bit NULL;
" );
        }

        public override void Down()
        {
        }
    }
}
