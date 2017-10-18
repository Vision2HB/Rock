using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 12, "1.5.0" )]
    class AddIndexes : Migration
    {
        public override void Up()
        {
            Sql( @"
    CREATE NONCLUSTERED INDEX [IX__org_willowcreek_CareCenter_ResourceProperty_DefinedValue] ON [dbo].[_org_willowcreek_CareCenter_ResourceProperty]
    (
	    [DefinedValueId] ASC,
	    [ResourceId] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

    CREATE NONCLUSTERED INDEX [IX__org_willowcreek_CareCenter_ResourceProperty_ResourceId] ON [dbo].[_org_willowcreek_CareCenter_ResourceProperty]
    (
	    [ResourceId] ASC,
	    [DefinedValueId] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
" );
        }

        public override void Down()
        {
        }
    }
}
