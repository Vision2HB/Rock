using Rock.Plugin;

namespace com.bemaservices.GroupTools
{
    [MigrationNumber( 10, "1.11.2" )]
    public class AttendanceEntry : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {

            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Attendance", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "", "", "Attendance Type", "", "", 1048, "", "097BFF6F-95BA-4627-AF91-45037F55FBB2", "AttendanceType" );
            RockMigrationHelper.AddAttributeQualifier( "097BFF6F-95BA-4627-AF91-45037F55FBB2", "values", "Virtual, In-person", "21880D97-98EB-445F-885F-4D9A8650E38A" );
            RockMigrationHelper.AddLayout( "F3F82256-2D66-432B-9D67-3552CD2F4C2B", "AttendancePage", "Attendance Page", "", "71137434-2283-4DDB-B577-CF57C9C381FC" );

            // Page: Attendance Entry
            RockMigrationHelper.AddPage( "4E237286-B715-4109-A578-C1445EC02707", "71137434-2283-4DDB-B577-CF57C9C381FC", "Attendance Entry", "", "D2E62A8A-33F2-4A6A-AF04-1188F5076176", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Group Attendance Entry", "Lists the group members for a specific occurrence datetime and allows selecting if they attended or not.", "~/Plugins/com_bemaservices/GroupTools/GroupAttendanceEntry.ascx", "BEMA Services > Groups", "16C7875E-587B-4BD9-9BA6-DF0368840917" );
            // Add Block to Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlock( true, "D2E62A8A-33F2-4A6A-AF04-1188F5076176", "", "16C7875E-587B-4BD9-9BA6-DF0368840917", "Group Attendance Entry", "Main", "", "", 0, "6741835A-0BE7-4A84-A555-8D276B0408E2" );
            // Add Block to Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlock( true, "D2E62A8A-33F2-4A6A-AF04-1188F5076176", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "CSS", "Main", "", "", 1, "577BD209-27A7-48EC-8F7B-D22F8DBA9B16" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: Group Attendance Entry:Allowed Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Allowed Group Types", "AllowedGroupTypes", "Allowed Group Types", @"", 0, @"", "33291D8D-6A6E-4321-9FEA-C830A724D764" );
            // Attrib for BlockType: Group Attendance Entry:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' attribute exista, its value will be set with the corresponding saved attendance value.", 1, @"", "DE739005-E9D6-42E9-843A-C0E7BF5ED4C1" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: Group Attendance Entry:Note Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "Note Types", @"The Note Types that can be added to a person's profile", 2, @"", "3B8AE2DB-5A5E-42E4-93B8-74F5CFFDF290" );
            // Attrib for BlockType: Group Attendance Entry:Note Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Note Workflow", "NoteWorkflow", "Note Workflow", @"An optional workflow type to launch whenever a note is saved. The Note will be used as the workflow 'Entity' when processing is started. ", 3, @"", "2D08709D-FEAA-4EB8-B451-C138C868E06F" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: Group Attendance Entry:Default Attendance Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Default Attendance Type", "DefaultAttendanceType", "Default Attendance Type", @"An optional default attendance type to use if one is not passed through the page parameters.", 4, @"", "5B0B907F-D1CC-4675-A605-FF0CD7509F61" );
            // Attrib for BlockType: Group Attendance Entry:Show Inactive Members
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Inactive Members", "ShowInactiveMembers", "Show Inactive Members", @"", 5, @"False", "5724B760-64D0-45D0-9BC0-1EE7DD4191B0" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: Group Attendance Entry:Show Pending Members
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Pending Members", "ShowPendingMembers", "Show Pending Members", @"", 6, @"False", "0E712920-80C1-44F5-BA01-3A62B5FA6CDB" );
            // Attrib for BlockType: Group Attendance Entry:Success Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Text", "SuccessText", "Success Text", @"The text to display after an attendance has been saved.", 7, @"Attendance Saved", "C025F394-03F0-4099-AA50-6E0A102C89BB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share HTML values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: Group Attendance Entry:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 8, @"368DD475-242C-49C4-A42C-7278BE690CC2", "ABBDC3AD-CEFB-4821-9573-ECE74F6B7F4A" );
            // Attrib for BlockType: Group Attendance Entry:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 9, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "DDB046E6-8AD9-40EC-82F3-A9431FDD9703" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: Group Attendance Entry:Restrict Future Occurrence Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Restrict Future Occurrence Date", "RestrictFutureOccurrenceDate", "Restrict Future Occurrence Date", @"Should user be prevented from selecting a future Occurrence date?", 10, @"False", "E1A21E8C-7ECC-4D2A-B482-94D399BAFC30" );
            // Attrib for BlockType: Group Attendance Entry:Allow Sorting
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16C7875E-587B-4BD9-9BA6-DF0368840917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Sorting", "AllowSorting", "Allow Sorting", @"Should the block allow sorting the Member's list by First Name or Last Name?", 11, @"True", "1F781AB0-0EF4-43F0-B229-7025C00E2C7B" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Workflow Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "DE739005-E9D6-42E9-843A-C0E7BF5ED4C1", @"cf9cf8bf-3616-4803-97af-f92e7bb6d86c" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Restrict Future Occurrence Date Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "E1A21E8C-7ECC-4D2A-B482-94D399BAFC30", @"False" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Allow Sorting Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "1F781AB0-0EF4-43F0-B229-7025C00E2C7B", @"True" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Note Types Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "3B8AE2DB-5A5E-42E4-93B8-74F5CFFDF290", @"87BACB34-DB87-45E0-AB60-BFABF7CEECDB,66A1B9D7-7EFA-40F3-9415-E54437977D60" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Note Workflow Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "2D08709D-FEAA-4EB8-B451-C138C868E06F", @"ade8246c-9165-459f-b15e-343954b6892f" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Default Attendance Type Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "5B0B907F-D1CC-4675-A605-FF0CD7509F61", @"" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Show Inactive Members Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "5724B760-64D0-45D0-9BC0-1EE7DD4191B0", @"True" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Show Pending Members Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "0E712920-80C1-44F5-BA01-3A62B5FA6CDB", @"True" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Success Text Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "C025F394-03F0-4099-AA50-6E0A102C89BB", @"Attendance Saved" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Record Status Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "DDB046E6-8AD9-40EC-82F3-A9431FDD9703", @"283999ec-7346-42e3-b807-bce9b2babb49" );
            // Attrib Value for Block:Group Attendance Entry, Attribute:Connection Status Page: Attendance Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "6741835A-0BE7-4A84-A555-8D276B0408E2", "ABBDC3AD-CEFB-4821-9573-ECE74F6B7F4A", @"368dd475-242c-49c4-a42c-7278be690cc2" );

            RockMigrationHelper.UpdateHtmlContentBlock( "577BD209-27A7-48EC-8F7B-D22F8DBA9B16", "{% include '/Plugins/com_bemaservices/GroupTools/Assets/Lava/AttendanceEntryCss.lava' %}", "3AE04FAD-D703-431A-B408-4AA01421791D" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}

