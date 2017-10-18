using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 5, "1.5.0" )]
    class AddSecurityRoles : Migration
    {
        public override void Up()
        {
            // add new note type
            Sql( @"
DECLARE @PersonEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )

IF  NOT EXISTS (SELECT * FROM [NoteType] WHERE [GUID] = '6E79DDC0-9A20-48D8-B5DE-C3A9F5636D42')
BEGIN
    INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [Name], [Guid], [UserSelectable], [IconCssClass], [EntityTypeQualifierColumn], [EntityTypeQualifierValue] )
    VALUES ( 1, @PersonEntityTypeId, 'Care Center: Care Team', '6E79DDC0-9A20-48D8-B5DE-C3A9F5636D42', 1, 'fa fa-users', '', '' )
END

IF  NOT EXISTS (SELECT * FROM [NoteType] WHERE [GUID] = 'D9234C0B-34AF-486A-A516-2DB9732EC22A')
BEGIN
    INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [Name], [Guid], [UserSelectable], [IconCssClass], [EntityTypeQualifierColumn], [EntityTypeQualifierValue] )
    VALUES ( 1, @PersonEntityTypeId, 'Care Center: Employment', 'D9234C0B-34AF-486A-A516-2DB9732EC22A', 1, 'fa fa-users', '', '' )
END

IF  NOT EXISTS (SELECT * FROM [NoteType] WHERE [GUID] = '208AB786-1CE9-42CB-93A4-5BB73AF41FE4')
BEGIN
    INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [Name], [Guid], [UserSelectable], [IconCssClass], [EntityTypeQualifierColumn], [EntityTypeQualifierValue] )
    VALUES ( 1, @PersonEntityTypeId, 'Care Center: Financial', '208AB786-1CE9-42CB-93A4-5BB73AF41FE4', 1, 'fa fa-users', '', '' )
END
" );
        }

        public override void Down()
        {
        }
    }
}
