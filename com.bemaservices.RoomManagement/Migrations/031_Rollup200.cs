// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 31, "1.10.3" )]
    public class Rollup200 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
               ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] ADD [OverrideApprovalGroupId] INT NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_OverrideApprovalGroupId] FOREIGN KEY([OverrideApprovalGroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_OverrideApprovalGroupId]             
" );

            Sql( @" UPDATE _com_bemaservices_RoomManagement_ReservationType
                    SET [OverrideApprovalGroupId] = [SuperAdminGroupId]" );
     
            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [ReservationId] [int] NOT NULL,
	                [EventItemOccurrenceId] [int] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationLinkage] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation] FOREIGN KEY([ReservationId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage] FOREIGN KEY([EventItemOccurrenceId])
                REFERENCES [dbo].[EventItemOccurrence] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_Linkage]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationLinkage_ModifiedByPersonAliasId]

" );
            RockMigrationHelper.UpdateNoteType( "Reservation Note", "com.bemaservices.RoomManagement.Model.Reservation", true, "2D02BFC9-EE35-4297-9957-146AF9EB1660" );

            // Page: New Reservation
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Notes", "Context aware block for adding notes to an entity.", "~/Blocks/Core/Notes.ascx", "Core", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3" );
            RockMigrationHelper.UpdateBlockType( "Reservation Detail", "Block for viewing a reservation detail", "~/Plugins/com_bemaservices/RoomManagement/ReservationDetail.ascx", "com_bemaservices > Room Management", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB" );
            // Add Block to Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4CBD2B96-E076-46DF-A576-356BCA5E577F", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Reservation Tabs", "Main", "", "", 1, "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0" );
            // Add Block to Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4CBD2B96-E076-46DF-A576-356BCA5E577F", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 2, "D7F1AD31-6E68-47E8-8C39-D29E42508FC0" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: Notes:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174" );
            // Attrib for BlockType: Notes:Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "", @"The text to display as the heading.  If left blank, the Note Type name will be used.", 1, @"", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: Notes:Heading Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading Icon CSS Class", "HeadingIcon", "", @"The css class name to use for the heading icon. ", 2, @"fa fa-sticky-note-o", "B69937BE-000A-4B94-852F-16DE92344392" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: Notes:Note Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Term", "NoteTerm", "", @"The term to use for note (i.e. 'Note', 'Comment').", 3, @"Note", "FD0727DC-92F4-4765-82CB-3A08B7D864F8" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: Notes:Display Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Type", "DisplayType", "", @"The format to use for displaying notes.", 4, @"Full", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E" );
            // Attrib for BlockType: Notes:Use Person Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Person Icon", "UsePersonIcon", "", @"", 5, @"False", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: Notes:Show Alert Checkbox
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Alert Checkbox", "ShowAlertCheckbox", "", @"", 6, @"True", "20243A98-4802-48E2-AF61-83956056AC65" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: Notes:Show Private Checkbox
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Private Checkbox", "ShowPrivateCheckbox", "", @"", 7, @"True", "D68EE1F5-D29F-404B-945D-AD0BE76594C3" );
            // Attrib for BlockType: Notes:Show Security Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Security Button", "ShowSecurityButton", "", @"", 8, @"True", "00B6EBFF-786D-453E-8746-119D0B45CB3E" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: Notes:Allow Anonymous
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Anonymous", "AllowAnonymous", "", @"", 9, @"False", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7" );
            // Attrib for BlockType: Notes:Add Always Visible
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Add Always Visible", "AddAlwaysVisible", "", @"Should the add entry screen always be visible (vs. having to click Add button to display the entry screen).", 10, @"False", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib for BlockType: Notes:Display Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Order", "DisplayOrder", "", @"Descending will render with entry field at top and most recent note at top.  Ascending will render with entry field at bottom and most recent note at the end.  Ascending will also disable the more option", 11, @"Descending", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1" );
            // Attrib for BlockType: Notes:Note Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "", @"Optional list of note types to limit display to", 12, @"", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4" );
            // Attrib for BlockType: Notes:Allow Backdated Notes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Backdated Notes", "AllowBackdatedNotes", "", @"", 12, @"False", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61" );
            // Attrib for BlockType: Notes:Display Note Type Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Note Type Heading", "DisplayNoteTypeHeading", "", @"Should each note's Note Type be displayed as a heading above each note?", 13, @"False", "C5FD0719-1E03-4C17-BE31-E02A3637C39A" );
            // Attrib for BlockType: Notes:Expand Replies
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Expand Replies", "ExpandReplies", "", @"Should replies to automatically expanded?", 14, @"False", "84E53A88-32D2-432C-8BB5-600BDBA10949" );
            // Attrib for BlockType: Notes:Note View Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Note View Lava Template", "NoteViewLavaTemplate", "", @"The Lava Template to use when rendering the readonly view of all the notes.", 15, @"{% include '~~/Assets/Lava/NoteViewList.lava' %}", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Start in Code Editor mode Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Cache Duration Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Require Approval Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enable Versioning Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Image Root Folder Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:User Specific Folders Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Document Root Folder Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Is Secondary Block Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enabled Lava Commands Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:Notes, Attribute:Note Types Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4", @"2D02BFC9-EE35-4297-9957-146AF9EB1660" );
            // Attrib Value for Block:Notes, Attribute:Display Note Type Heading Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "C5FD0719-1E03-4C17-BE31-E02A3637C39A", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note View Lava Template Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307", @"{% include '~~/Assets/Lava/NoteViewList.lava' %}" );
            // Attrib Value for Block:Notes, Attribute:Expand Replies Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "84E53A88-32D2-432C-8BB5-600BDBA10949", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Backdated Notes Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" );
            // Attrib Value for Block:Notes, Attribute:Entity Type Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"839768a3-10d6-446c-a65b-b8f9efd7808f" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-sticky-note-o" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D7F1AD31-6E68-47E8-8C39-D29E42508FC0", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Add/Update PageContext for Page:New Reservation, Entity: com.bemaservices.RoomManagement.Model.Reservation, Parameter: ReservationId
            RockMigrationHelper.UpdatePageContext( "4CBD2B96-E076-46DF-A576-356BCA5E577F", "com.bemaservices.RoomManagement.Model.Reservation", "ReservationId", "18F65AE1-1485-40CD-B63C-7EC27196B4E5" );


            // Page: History
            RockMigrationHelper.AddPage( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "History", "", "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "History Log", "Block for displaying the history of changes to a particular entity.", "~/Blocks/Core/HistoryLog.ascx", "Core", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0" );
            RockMigrationHelper.UpdateBlockType( "Reservation Detail", "Block for viewing a reservation detail", "~/Plugins/com_bemaservices/RoomManagement/ReservationDetail.ascx", "com_bemaservices > Room Management", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB" );
            // Add Block to Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "Reservation Detail", "Main", "", "", 0, "178C1B2A-1371-46AA-9BAD-2FC4A423DE9A" );
            // Add Block to Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Reservation Tabs", "Main", "", "", 1, "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1" );
            // Add Block to Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 2, "3CFA3A78-8CBB-49EF-8195-F7304A554B12" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: History Log:Heading
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "", @"The Lava template to use for the heading. <span class='tip tip-lava'></span>", 0, @"{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047" );
            // Attrib for BlockType: History Log:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "8FB690EC-5299-46C5-8695-AAD23168E6E1" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Cache Duration Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Require Approval Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enable Versioning Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Start in Code Editor mode Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Image Root Folder Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:User Specific Folders Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Document Root Folder Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enabled Lava Commands Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Is Secondary Block Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            // Attrib Value for Block:History Log, Attribute:Heading Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3CFA3A78-8CBB-49EF-8195-F7304A554B12", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"{{ Entity.Name}} (ID:{{ Entity.Id }})" );
            // Attrib Value for Block:History Log, Attribute:Entity Type Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3CFA3A78-8CBB-49EF-8195-F7304A554B12", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"839768a3-10d6-446c-a65b-b8f9efd7808f" );
            // Add/Update PageContext for Page:History, Entity: com.bemaservices.RoomManagement.Model.Reservation, Parameter: ReservationId
            RockMigrationHelper.UpdatePageContext( "FF1AA1C0-4142-45C8-9F3B-00632CB10E89", "com.bemaservices.RoomManagement.Model.Reservation", "ReservationId", "74CC7218-20E3-4ACA-8235-799FCC2E11B7" );

            // Page: Events
            RockMigrationHelper.AddPage( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Events", "", "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.UpdateBlockType( "Reservation Detail", "Block for viewing a reservation detail", "~/Plugins/com_bemaservices/RoomManagement/ReservationDetail.ascx", "com_bemaservices > Room Management", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB" );
            RockMigrationHelper.UpdateBlockType( "Reservation Linkage List", "Displays the linkages associated with a reservation.", "~/Plugins/com_bemaservices/RoomManagement/ReservationLinkageList.ascx", "BEMA Services > Room Management", "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C" );
            // Add Block to Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "Reservation Detail", "Main", "", "", 0, "94BDF4E7-CA48-4C66-B0FB-5B13B87A5E0A" );
            // Add Block to Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Reservation Tabs", "Main", "", "", 1, "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306" );
            // Add Block to Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "", "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "Reservation Linkage List", "Main", "", "", 2, "4F6E7570-FD87-4991-8C6F-8E2568B1377E" );
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );
            // Attrib for BlockType: Reservation Linkage List:Linkage Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Linkage Page", "LinkagePage", "Linkage Page", @"The page for creating a reservation linkage", 1, @"DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E", "95759D3F-5E2C-44B7-9C13-C53D78916A56" );
            // Attrib for BlockType: Reservation Linkage List:Linkage Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Linkage Page", "LinkagePage", "Linkage Page", @"The page for creating a reservation linkage", 1, @"DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E", "97F12733-48A1-42CF-9CAD-254609D25D13" );
            // Attrib for BlockType: Reservation Linkage List:Calendar Item Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Calendar Item Page", "CalendarItemDetailPage", "Calendar Item Page", @"The page to view calendar item details", 2, @"7FB33834-F40A-4221-8849-BB8C06903B04", "28863235-DE8C-402E-B227-A91D3827AD54" );
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            // Attrib for BlockType: Reservation Linkage List:Content Item Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "106C896B-B3AA-42ED-91E8-5F5A1ED6B42C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Page", "ContentItemDetailPage", "Content Item Page", @"The page for viewing details about a content channel item", 3, @"D18E837C-9E65-4A38-8647-DFF04A595D97", "0E06771C-3522-466E-BBE6-5578ED22C3FF" );
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Cache Duration Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Require Approval Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enable Versioning Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Start in Code Editor mode Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Image Root Folder Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:User Specific Folders Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Document Root Folder Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Enabled Lava Commands Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"RockEntity" );
            // Attrib Value for Block:Reservation Tabs, Attribute:Is Secondary Block Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );
            // Attrib Value for Block:Reservation Linkage List, Attribute:Linkage Page Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4F6E7570-FD87-4991-8C6F-8E2568B1377E", "95759D3F-5E2C-44B7-9C13-C53D78916A56", @"2d25f333-4f47-462b-94c0-6771abf426d6" );
            // Attrib Value for Block:Reservation Linkage List, Attribute:Content Item Page Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4F6E7570-FD87-4991-8C6F-8E2568B1377E", "0E06771C-3522-466E-BBE6-5578ED22C3FF", @"d18e837c-9e65-4a38-8647-dff04a595d97" );
            // Attrib Value for Block:Reservation Linkage List, Attribute:Calendar Item Page Page: Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4F6E7570-FD87-4991-8C6F-8E2568B1377E", "28863235-DE8C-402E-B227-A91D3827AD54", @"7fb33834-f40a-4221-8849-bb8c06903b04" );

            // Page: Linkage Detail
            RockMigrationHelper.AddPage( "6F74FD8C-2478-46A2-B26F-5D0D052B4BC2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Linkage Detail", "", "2D25F333-4F47-462B-94C0-6771ABF426D6", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Reservation Linkage Detail", "Block for creating a Reservation Linkage", "~/Plugins/com_bemaservices/RoomManagement/ReservationLinkageDetail.ascx", "BEMA Services > Room Management", "B25263A0-F51B-4F62-A402-973707528572" );
            // Add Block to Page: Linkage Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2D25F333-4F47-462B-94C0-6771ABF426D6", "", "B25263A0-F51B-4F62-A402-973707528572", "Reservation Linkage Detail", "Main", "", "", 0, "95B727BB-3ADC-4106-8CE0-41B522FC030E" );
            // Attrib for BlockType: Reservation Linkage Detail:Default Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "EC0D9528-1A22-404E-A776-566404987363", "Default Calendar", "DefaultCalendar", "Default Calendar", @"The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.", 0, @"", "CE976B3C-EBAD-4A7E-8E31-CCF23B6B694E" );
            // Attrib for BlockType: Reservation Linkage Detail:Default Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "EC0D9528-1A22-404E-A776-566404987363", "Default Calendar", "DefaultCalendar", "Default Calendar", @"The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.", 0, @"", "1A5649D4-EE00-46AE-B16C-3A49077D7354" );
            // Attrib for BlockType: Reservation Linkage Detail:Allow Creating New Calendar Events
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Creating New Calendar Events", "AllowCreatingNewCalendarEvents", "Allow Creating New Calendar Events", @"If set to ""Yes"", the staff person will be offered the ""New Event"" tab to create a new event and a new occurrence of that event, rather than only picking from existing events.", 1, @"true", "55AABE18-39B3-4DFD-BF0B-057714CFCE5B" );
            // Attrib for BlockType: Reservation Linkage Detail:Include Inactive Calendar Items
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Calendar Items", "IncludeInactiveCalendarItems", "Include Inactive Calendar Items", @"Check this box to hide inactive calendar items.", 2, @"true", "226AFC08-CB47-46E8-A9F2-C51475EFA69E" );
            // Attrib for BlockType: Reservation Linkage Detail:Completion Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Completion Workflow", "CompletionWorkflow", "Completion Workflow", @"A workflow that will be launched when the wizard is complete.  The following attributes will be passed to the workflow:
 + Reservation
 + EventItemOccurrenceGuid", 3, @"", "FDB1410F-1D7F-4B86-87DC-F445154CD44C" );
            // Attrib for BlockType: Reservation Linkage Detail:Event Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Event Instructions Lava Template", "LavaInstruction_Event", "Event Instructions Lava Template", @"Instructions here will show up on the fourth panel of the wizard.", 4, @"", "6AD8EB95-A5DB-4302-B85D-BABA0C59E088" );
            // Attrib for BlockType: Reservation Linkage Detail:Display Link to Event Details Page on Confirmation Screen
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Link to Event Details Page on Confirmation Screen", "DisplayEventDetailsLink", "Display Link to Event Details Page on Confirmation Screen", @"Check this box to show the link to the event details page in the wizard confirmation screen.", 4, @"true", "93D9EAAB-5672-485A-A9FC-77E73D5EB6CF" );
            // Attrib for BlockType: Reservation Linkage Detail:External Event Details Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "External Event Details Page", "EventDetailsPage", "External Event Details Page", @"Determines which page the link in the final confirmation screen will take you to (if ""Display Link to Event Details ... "" is selected).", 5, @"8A477CC6-4A12-4FBE-8037-E666476DD413", "4768E7F8-3BC5-46DF-9AFF-38244FEDA336" );
            // Attrib for BlockType: Reservation Linkage Detail:External Event Details Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "External Event Details Page", "EventDetailsPage", "External Event Details Page", @"Determines which page the link in the final confirmation screen will take you to (if ""Display Link to Event Details ... "" is selected).", 5, @"8A477CC6-4A12-4FBE-8037-E666476DD413", "420A1979-A875-4248-838B-25E040FBBE3A" );
            // Attrib for BlockType: Reservation Linkage Detail:Event Occurrence Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Event Occurrence Instructions Lava Template", "LavaInstruction_EventOccurrence", "Event Occurrence Instructions Lava Template", @"Instructions here will show up on the fifth panel of the wizard.", 5, @"", "D7EE02D0-C8C7-451F-925B-9C2BF0FD3F2E" );
            // Attrib for BlockType: Reservation Linkage Detail:Summary Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Summary Instructions Lava Template", "LavaInstruction_Summary", "Summary Instructions Lava Template", @"Instructions here will show up on the sixth panel of the wizard.", 6, @"", "74024AF7-EEF0-4C9B-A90F-19DAF2DAD453" );
            // Attrib for BlockType: Reservation Linkage Detail:Summary Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Summary Instructions Lava Template", "LavaInstruction_Summary", "Summary Instructions Lava Template", @"Instructions here will show up on the sixth panel of the wizard.", 6, @"", "772FFED2-C8C5-4F40-87A9-BFC23E9CA08B" );
            // Attrib for BlockType: Reservation Linkage Detail:Wizard Finished Instructions Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B25263A0-F51B-4F62-A402-973707528572", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Wizard Finished Instructions Lava Template", "LavaInstruction_Finished", "Wizard Finished Instructions Lava Template", @"Instructions here will show up on the final panel of the wizard.", 7, @"", "93654146-7156-4A9F-A88A-62C501836A17" );


            RockMigrationHelper.UpdateHtmlContentBlock( "5859D1E7-A1CA-452F-BF40-6E0F5E9011E0", "{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/ReservationTabs.lava' %}", "4D5B4438-D3A3-468F-BCAD-F7A0DC36F2F3" );
            RockMigrationHelper.UpdateHtmlContentBlock( "8EB8B82E-5BD9-41F8-AC1F-2756A83DB306", "{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/ReservationTabs.lava' %}", "E1A199D7-AA1F-4F8E-94EC-C3EE8386E2B6" );
            RockMigrationHelper.UpdateHtmlContentBlock( "33F5BEFC-41FF-406C-AF1C-2EC5323BE1E1", "{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/ReservationTabs.lava' %}", "16A29568-775C-4E14-9EC4-B822D3B1C6A4" );

            RockMigrationHelper.DeleteBlock( "A981B5ED-F5B4-41AE-96A3-2BC10CCF110B" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}