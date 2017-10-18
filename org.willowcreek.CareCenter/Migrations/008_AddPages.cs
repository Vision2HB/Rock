using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 8, "1.5.0" )]
    class AddPages : Migration
    {
        public override void Up()
        {
            AddSite();
            AddLayouts();
            AddHomePage();
            UpdateSite();
        }

        private void AddSite()
        {
            RockMigrationHelper.AddSite( "Care Center", "Internal portal for the Care Center", "CareCenter", SystemGuid.Site.SITE_CARE_CENTER_INTERNAL );
        }

        private void UpdateSite()
        { 
            Sql( @"
    UPDATE [Site] SET
	     [DefaultPageId] = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'FD92FE87-D749-40F6-8161-77CFD8431AFB')
        ,[LoginPageId] =  (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '3012B22F-912F-4C47-B8BC-219E1A0CE692')
        ,[ChangePasswordPageId] = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'D4E16ECC-FC79-412B-A35C-F9147E9341A2')
        ,[ErrorPage] = '~/Themes/CareCenter/Layouts/Error.aspx'
	WHERE [Guid] = 'D1F230C2-6485-B586-43F4-FFADA0FA08F6'
" );
        }

        private void AddLayouts()
        {
            // Blank Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "Blank", "Blank", "", "08C060A1-021D-4678-8C96-28D5268B1B76" ); // Site:Care Center Internal

            // Dialog Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "Dialog", "Dialog", "", "8318D144-F1E5-48FB-87EA-B0A751282833" ); // Site:Care Center Internal

            // Error Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "Error", "Error", "", "E8FFC373-3736-4DC9-8BBF-FF57EFEAAC9E" ); // Site:Care Center Internal

            // FullWidth Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "FullWidth", "Full Width", "", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC" ); // Site:Care Center Internal

            RockMigrationHelper.AddBlock( "", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Header Text", "Header", "", "", 0, "7F835289-861E-4499-9DAD-D7BA2E10A462" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "1F3F1168-71B9-434A-AC77-67D3C29027F8", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "466993F7-D838-447A-97E7-8BBDA6A57289", @"CareCenterHeader" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "7F835289-861E-4499-9DAD-D7BA2E10A462", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            RockMigrationHelper.AddBlock( "", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "04712F3D-9667-4901-A49D-4507573EF7AD", "Login", "Login", "", "", 0, "96BB4F92-BF54-4FC9-A01B-5FF3F6148441" );

            RockMigrationHelper.AddBlock( "", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer", "Footer", "", "", 0, "37201C11-CFDA-487A-8531-F5F123361718" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "466993F7-D838-447A-97E7-8BBDA6A57289", @"CareCenterFooter" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "37201C11-CFDA-487A-8531-F5F123361718", "1F3F1168-71B9-434A-AC77-67D3C29027F8", @"" );
            RockMigrationHelper.UpdateHtmlContentBlock( "37201C11-CFDA-487A-8531-F5F123361718", @"© <script>document.write(new Date().getFullYear())</script> Willow Creek Community Church | 847-765-5000<br /><small>Powered By: Rock RMS</small>", "B5571974-E543-4AB8-908D-10F83111492C" );
            Sql( "UPDATE [HtmlContent] SET [EntityValue] = '&ContextName=CareCenterFooter' WHERE [Guid] = 'B5571974-E543-4AB8-908D-10F83111492C'" );

            // Left Sidebar Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "LeftSidebar", "Left Sidebar", "", "4B5A029F-E372-4991-9BE7-107D3D5BC042" ); // Site:Care Center Internal

            // Right Sidebar Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "RightSidebar", "Right Sidebar", "", "CF031F3F-3375-4A29-93FC-A97B15864208" ); // Site:Care Center Internal

            // Three Column Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "ThreeColumn", "Three Column", "", "39146627-5245-47BB-8C0B-7F22DA12AC26" ); // Site:Care Center Internal

            // Homepage Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "Homepage", "Homepage", "", "E3EA9367-3848-498F-41EB-30E7D452DA3C" ); // Site:Care Center Internal

            RockMigrationHelper.AddBlock( "", "E3EA9367-3848-498F-41EB-30E7D452DA3C", "04712F3D-9667-4901-A49D-4507573EF7AD", "Login", "Login", "", "", 0, "353F8BF7-BCEA-4873-AA39-EB702A1B3AC9" );

            RockMigrationHelper.AddBlock( "", "E3EA9367-3848-498F-41EB-30E7D452DA3C", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer", "Footer", "", "", 0, "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "466993F7-D838-447A-97E7-8BBDA6A57289", @"CareCenterFooter" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", "1F3F1168-71B9-434A-AC77-67D3C29027F8", @"" );
            RockMigrationHelper.UpdateHtmlContentBlock( "3F607FA8-CED3-4597-B8CD-E81C4FCC0D2D", @"Powered by: <a href=""http://www.rockrms.com"">Rock RMS</a>", "6092D6EE-A47C-40B9-B4E7-6409BC219254" );
            Sql( "UPDATE [HtmlContent] SET [EntityValue] = '&ContextName=CareCenterFooter' WHERE [Guid] = '6092D6EE-A47C-40B9-B4E7-6409BC219254'" );


            // Person Profile Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "PersonProfile", "Person Profile", "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0" ); // Site:Care Center Internal

            RockMigrationHelper.AddBlock( "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "04712F3D-9667-4901-A49D-4507573EF7AD", "Login", "Login", "", "", 0, "8F901430-B6E3-4E38-B925-E30018D76435" );

            RockMigrationHelper.AddBlock( "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer", "Footer", "", "", 0, "14BB8D13-C50E-4CF2-BAB1-0B8804D91646" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "466993F7-D838-447A-97E7-8BBDA6A57289", @"CareCenterFooter" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", "1F3F1168-71B9-434A-AC77-67D3C29027F8", @"" );
            RockMigrationHelper.UpdateHtmlContentBlock( "14BB8D13-C50E-4CF2-BAB1-0B8804D91646", @"Powered by: <a href=""http://www.rockrms.com"">Rock RMS</a>", "4ACD6F45-28FD-44D2-ACBC-BF67EBE81189" );
            Sql( "UPDATE [HtmlContent] SET [EntityValue] = '&ContextName=CareCenterFooter' WHERE [Guid] = '4ACD6F45-28FD-44D2-ACBC-BF67EBE81189'" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Custom Content", "CustomContent", "", "Custom Content will be rendered after the person's demographic information <span class='tip tip-lava'></span>.", 6, @"", "921C9ED8-483D-4532-BAC5-437E08516311" );

            RockMigrationHelper.AddBlock( "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "IndividualDetail", "", "", 0, "4E397A53-2E0E-4419-B6A7-F46CCC807A31" );
            RockMigrationHelper.AddBlockAttributeValue( "4E397A53-2E0E-4419-B6A7-F46CCC807A31", "8E11F65B-7272-4E9F-A4F1-89CE08E658DE", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "4E397A53-2E0E-4419-B6A7-F46CCC807A31", "35F69669-48DE-4182-B828-4EC9C1C31B08", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "4E397A53-2E0E-4419-B6A7-F46CCC807A31", "7197A0FB-B330-43C4-8E62-F3C14F649813", @"221bf486-a82c-40a7-85b7-bb44da45582f" );
            RockMigrationHelper.AddBlockAttributeValue( "4E397A53-2E0E-4419-B6A7-F46CCC807A31", "879ED630-23C8-429D-A064-32168DB8057C", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "4E397A53-2E0E-4419-B6A7-F46CCC807A31", "384EA763-B8E5-4A41-997F-ACD1B002AF8D", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "4E397A53-2E0E-4419-B6A7-F46CCC807A31", "921C9ED8-483D-4532-BAC5-437E08516311", @"<div class='actions pull-right'>
    <a class='btn btn-action btn-sm' href='/Intake/{{ Context.Person.Id }}'>Intake</a>
    <a class='btn btn-action btn-sm' href='/Appointment/Add/{{ Context.Person.Id }}'>Add Appointment</a>
    <a class='btn btn-action btn-sm' href='/Assessment/{{ Context.Person.Id }}'>Start Assessment</a>
    <a class='btn btn-action btn-sm' href='/CarePastor/{{ Context.Person.Id }}'>Care Pastor</a>
</div>" );

            RockMigrationHelper.AddBlock( "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges", "BadgeBarLeft", "", "", 0, "BE30C279-8084-453E-9CEC-D3838B4F5CD6" );

            RockMigrationHelper.AddBlock( "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges", "BadgeBarMiddle", "", "", 0, "083F096B-3F02-43C3-9BBC-17AC52E69EBF" );

            RockMigrationHelper.AddBlock( "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "Badges", "BadgeBarRight", "", "", 0, "C1915885-3046-4EDE-BE0A-EEE8A47CFF75" );

            RockMigrationHelper.AddBlock( "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "Group Members", "FamilyDetail", "", "", 0, "4DFF43B6-0E08-48E3-AA8E-09E9B2D1205C" );
            RockMigrationHelper.AddBlockAttributeValue( "4DFF43B6-0E08-48E3-AA8E-09E9B2D1205C", "F988BC15-4D12-4D37-9690-770394FDCB24", @"790e3215-3b10-442b-af69-616c0dcb998e" );
            RockMigrationHelper.AddBlockAttributeValue( "4DFF43B6-0E08-48E3-AA8E-09E9B2D1205C", "C12B3192-4B51-4733-AE9F-8D2D46B82DA9", @"6ec89e41-8c1d-4435-b5cd-0f8d876379c6" );
            RockMigrationHelper.AddBlockAttributeValue( "4DFF43B6-0E08-48E3-AA8E-09E9B2D1205C", "A1C5EAB7-B507-4DB7-916D-64A58EEF8691", @"47c5937b-2b96-4167-88a9-9bf08b2ec496" );
            RockMigrationHelper.AddBlockAttributeValue( "4DFF43B6-0E08-48E3-AA8E-09E9B2D1205C", "F005C18A-91D9-417E-AF14-3699E8D3A1CF", @"True" );

            RockMigrationHelper.AddBlock( "", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "SubNavigation", "", "", 0, "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"1b60ab7c-b1aa-40f9-b1a1-f0063b2229ee" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "900C98CC-EACF-4677-A8A5-9DEAAACCAD3E", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Assesment Layout
            RockMigrationHelper.AddLayout( "D1F230C2-6485-B586-43F4-FFADA0FA08F6", "PersonProfile", "Assesment", "Version of person profile layout used for assesment", "1AEF1CD1-2870-4027-8114-4D23972443F4" ); // Site:Care Center Internal

            RockMigrationHelper.AddBlock( "", "1AEF1CD1-2870-4027-8114-4D23972443F4", "04712F3D-9667-4901-A49D-4507573EF7AD", "Login", "Login", "", "", 0, "EB5254DF-C737-4605-B45A-A93DA931266C" );

            RockMigrationHelper.AddBlock( "", "1AEF1CD1-2870-4027-8114-4D23972443F4", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Footer", "Footer", "", "", 0, "D63E305B-B20A-4186-B7AC-9C8DC553AE11" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "466993F7-D838-447A-97E7-8BBDA6A57289", @"CareCenterFooter" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", "1F3F1168-71B9-434A-AC77-67D3C29027F8", @"" );
            RockMigrationHelper.UpdateHtmlContentBlock( "D63E305B-B20A-4186-B7AC-9C8DC553AE11", @"Powered by: <a href=""http://www.rockrms.com"">Rock RMS</a>", "81076654-EB77-4FEC-8F54-1887BBB8DBB9" );
            Sql( "UPDATE [HtmlContent] SET [EntityValue] = '&ContextName=CareCenterFooter' WHERE [Guid] = '81076654-EB77-4FEC-8F54-1887BBB8DBB9'" );

            RockMigrationHelper.AddBlock( "", "1AEF1CD1-2870-4027-8114-4D23972443F4", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "SubNavigation", "", "", 0, "4813FD6B-6500-4C6B-A058-FF43005181BC" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"284bb639-cf41-49d8-b6ab-5dfa4590a8b1" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "4813FD6B-6500-4C6B-A058-FF43005181BC", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );


        }

        private void AddHomePage()
        {
            RockMigrationHelper.AddPage( "", "E3EA9367-3848-498F-41EB-30E7D452DA3C", "Care Center Home Page", "", "FD92FE87-D749-40F6-8161-77CFD8431AFB", "" ); // Site:Care Center Internal

            Sql( @"
    UPDATE [Page] SET
	     [PageDisplayTitle] = 0
        ,[PageDisplayBreadCrumb] = 0
        ,[PageTitle] =  'Home'
	WHERE [Guid] = 'FD92FE87-D749-40F6-8161-77CFD8431AFB'
" );
            RockMigrationHelper.AddPageRoute( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "CareCenter", "C6E8A734-3590-47D3-9248-7589D46B47B4" );// for Page:Home

            RockMigrationHelper.AddBlock( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Feature", "", "", 0, "80FEFA50-4102-4884-9852-16C36B909AD3" );
            Sql( @" UPDATE [Block]
                  SET [PreHtml] = '<div class=""search-wrapper"">',
                  [PostHtml] = '</div>'
                  WHERE [Guid] = '80FEFA50-4102-4884-9852-16C36B909AD3'" );

            RockMigrationHelper.AddBlock( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "436CA03B-14E4-44F1-80BF-DCD05DE0D524" );
            RockMigrationHelper.AddBlockAttributeValue( "436CA03B-14E4-44F1-80BF-DCD05DE0D524", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "436CA03B-14E4-44F1-80BF-DCD05DE0D524", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "436CA03B-14E4-44F1-80BF-DCD05DE0D524", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "436CA03B-14E4-44F1-80BF-DCD05DE0D524", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "436CA03B-14E4-44F1-80BF-DCD05DE0D524", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );
            RockMigrationHelper.AddBlockAttributeValue( "436CA03B-14E4-44F1-80BF-DCD05DE0D524", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "436CA03B-14E4-44F1-80BF-DCD05DE0D524", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "436CA03B-14E4-44F1-80BF-DCD05DE0D524", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            AddIntakePage();
            AddAppointmentsPage();
            AddDashboardsPage();
            AddVisitsPage();
            AddAssessmentsPage();
            AddCarePastorPage();
            AddResourcesPage ();
            AddEventRegistrationPages();
            AddAdministrationPages();

            AddSupportPage();
        }

        #region Intake Pages

        private void AddIntakePage()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Intake", "", "A5B11B6A-6F8B-48DE-8BBB-65B5526F7F75", "fa fa-search" );

            RockMigrationHelper.UpdateBlockType( "Search", "Care Center search block", "~/Plugins/org_willowcreek/CareCenter/Search.ascx", "org_willowcreek > CareCenter", "CAF83DCB-70DA-495B-B478-E9CA4ED7E290" );
            RockMigrationHelper.AddBlockTypeAttribute( "CAF83DCB-70DA-495B-B478-E9CA4ED7E290", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Caption", "SearchCaption", "", "Caption to display at the top of search panel", 0, @"Find A Guest", "7181E372-0A8E-4881-93BE-E3F13F14CB49" );
            RockMigrationHelper.AddBlockTypeAttribute( "CAF83DCB-70DA-495B-B478-E9CA4ED7E290", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Results Caption", "ResultsCaption", "", "Caption to display at the top of search results", 1, @"Results", "B0E936AD-9E39-435C-B154-2F4E6700C990" );
            RockMigrationHelper.AddBlockTypeAttribute( "CAF83DCB-70DA-495B-B478-E9CA4ED7E290", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Family Page", "AddFamilyPage", "", "Page to navigate to when adding a new family", 4, @"6A11A13D-05AB-4982-A4C2-67A8B1950C74", "3B158A42-B467-4088-918D-950829C981F3" );
            RockMigrationHelper.AddBlockTypeAttribute( "CAF83DCB-70DA-495B-B478-E9CA4ED7E290", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Select Page", "SelectPage", "", "Page to navigate to after selecting a person", 2, @"", "F53D4A3A-82D7-4342-A2C1-181B9C0AE72D" );
            RockMigrationHelper.AddBlockTypeAttribute( "CAF83DCB-70DA-495B-B478-E9CA4ED7E290", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page to navigate to for viewing profile", 3, @"08DBD8A5-2C35-4146-B4A8-0F7652348B25", "E0A50169-4AEC-462E-84CA-B5BB16B216D3" );

            RockMigrationHelper.AddBlock( "A5B11B6A-6F8B-48DE-8BBB-65B5526F7F75", "", "CAF83DCB-70DA-495B-B478-E9CA4ED7E290", "Search", "Main", "", "", 0, "56A0383D-92E7-4735-8442-4B18FA5EFD9D" );
            RockMigrationHelper.AddBlockAttributeValue( "56A0383D-92E7-4735-8442-4B18FA5EFD9D", "7181E372-0A8E-4881-93BE-E3F13F14CB49", @"Find A Guest" ); // Search Caption
            RockMigrationHelper.AddBlockAttributeValue( "56A0383D-92E7-4735-8442-4B18FA5EFD9D", "B0E936AD-9E39-435C-B154-2F4E6700C990", @"Results" ); // Results Caption
            RockMigrationHelper.AddBlockAttributeValue( "56A0383D-92E7-4735-8442-4B18FA5EFD9D", "3B158A42-B467-4088-918D-950829C981F3", @"ff558ff5-3bd9-4e13-aa50-7f8417a88916,8a0e2d4b-e16c-4ba3-a7c8-18bb8e4cce17" ); // Add Family Page
            RockMigrationHelper.AddBlockAttributeValue( "56A0383D-92E7-4735-8442-4B18FA5EFD9D", "F53D4A3A-82D7-4342-A2C1-181B9C0AE72D", @"b3322488-717f-459f-b63f-813c68a78db9" ); // Select Page
            RockMigrationHelper.AddBlockAttributeValue( "56A0383D-92E7-4735-8442-4B18FA5EFD9D", "E0A50169-4AEC-462E-84CA-B5BB16B216D3", @"b3322488-717f-459f-b63f-813c68a78db9,f63deb87-96d5-4480-9688-5a4d43904301" ); // Person Profile Page
        }

        #endregion

        #region Appointments Pages

        private void AddAppointmentsPage()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Appointments", "", "E20405DB-F277-48BD-8455-85B38421A36B", "fa fa-calendar" );

            RockMigrationHelper.AddBlock( "E20405DB-F277-48BD-8455-85B38421A36B", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "ECDB7180-8408-423D-9901-17DE0313580C" );
            RockMigrationHelper.AddBlockAttributeValue( "ECDB7180-8408-423D-9901-17DE0313580C", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File
            RockMigrationHelper.AddBlockAttributeValue( "ECDB7180-8408-423D-9901-17DE0313580C", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters
            RockMigrationHelper.AddBlockAttributeValue( "ECDB7180-8408-423D-9901-17DE0313580C", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" ); // Template
            RockMigrationHelper.AddBlockAttributeValue( "ECDB7180-8408-423D-9901-17DE0313580C", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels
            RockMigrationHelper.AddBlockAttributeValue( "ECDB7180-8408-423D-9901-17DE0313580C", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString
            RockMigrationHelper.AddBlockAttributeValue( "ECDB7180-8408-423D-9901-17DE0313580C", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue( "ECDB7180-8408-423D-9901-17DE0313580C", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block
            RockMigrationHelper.AddBlockAttributeValue( "ECDB7180-8408-423D-9901-17DE0313580C", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" ); // Include Page List

            AddAppointmentSearchPage();
            AddAppointmentListPage();
            AddAppointmentEntryPage();
        }

        private void AddAppointmentSearchPage()
        {
            RockMigrationHelper.AddPage( "E20405DB-F277-48BD-8455-85B38421A36B", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Add Appointment", "", "0DEA2210-D5CA-4FD7-A022-F2D627FA3779", "fa fa-calendar-plus-o" ); 

            RockMigrationHelper.AddBlock( "0DEA2210-D5CA-4FD7-A022-F2D627FA3779", "", "CAF83DCB-70DA-495B-B478-E9CA4ED7E290", "Search", "Main", "", "", 0, "1BB47023-7320-4A10-BC03-FDD5F0837AD5" );
            RockMigrationHelper.AddBlockAttributeValue( "1BB47023-7320-4A10-BC03-FDD5F0837AD5", "7181E372-0A8E-4881-93BE-E3F13F14CB49", @"Find A Guest" ); // Search Caption
            RockMigrationHelper.AddBlockAttributeValue( "1BB47023-7320-4A10-BC03-FDD5F0837AD5", "B0E936AD-9E39-435C-B154-2F4E6700C990", @"Results" ); // Results Caption
            RockMigrationHelper.AddBlockAttributeValue( "1BB47023-7320-4A10-BC03-FDD5F0837AD5", "3B158A42-B467-4088-918D-950829C981F3", @"ff558ff5-3bd9-4e13-aa50-7f8417a88916,8a0e2d4b-e16c-4ba3-a7c8-18bb8e4cce17" ); // Add Family Page
            RockMigrationHelper.AddBlockAttributeValue( "1BB47023-7320-4A10-BC03-FDD5F0837AD5", "F53D4A3A-82D7-4342-A2C1-181B9C0AE72D", @"b3322488-717f-459f-b63f-813c68a78db9" ); // Select Page
            RockMigrationHelper.AddBlockAttributeValue( "1BB47023-7320-4A10-BC03-FDD5F0837AD5", "E0A50169-4AEC-462E-84CA-B5BB16B216D3", @"b3322488-717f-459f-b63f-813c68a78db9,f63deb87-96d5-4480-9688-5a4d43904301" ); // Person Profile Page
        }

        private void AddAppointmentListPage()
        {
            RockMigrationHelper.AddPage( "E20405DB-F277-48BD-8455-85B38421A36B", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "View Appointments", "", "AED66D1B-97B8-491A-B9BD-A7CFD4CC182F", "fa fa-calendar-check-o" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Appointment List", "Care Center block for viewing list of appointments.", "~/Plugins/org_willowcreek/CareCenter/AppointmentList.ascx", "org_willowcreek > CareCenter", "B345329A-B428-4C69-9B8E-E1FB4763B529" );
            RockMigrationHelper.AddBlockTypeAttribute( "B345329A-B428-4C69-9B8E-E1FB4763B529", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page for displaying details of a appointment", 0, @"", "408F85C8-9215-45C7-9B69-CE502F742570" );

            RockMigrationHelper.AddBlock( "AED66D1B-97B8-491A-B9BD-A7CFD4CC182F", "", "B345329A-B428-4C69-9B8E-E1FB4763B529", "Appointment List", "Main", "", "", 0, "C29147D3-E545-4E5D-957A-C343BD471E27" );
            RockMigrationHelper.AddBlockAttributeValue( "C29147D3-E545-4E5D-957A-C343BD471E27", "408F85C8-9215-45C7-9B69-CE502F742570", @"fbc943c6-52f4-4b47-a881-add53cf7bb1b" ); // Detail Page

            AddAppointmentDetailPage();
        }

        private void AddAppointmentDetailPage()
        {
            RockMigrationHelper.AddPage( "AED66D1B-97B8-491A-B9BD-A7CFD4CC182F", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Appointment Detail", "", "FBC943C6-52F4-4B47-A881-ADD53CF7BB1B", "fa fa-calendar" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Appointment Detail", "Care Center block for viewing/editing details of a appointment.", "~/Plugins/org_willowcreek/CareCenter/AppointmentDetail.ascx", "org_willowcreek > CareCenter", "615506A5-DCFF-4842-A65E-C48E20FA8FCA" );
            RockMigrationHelper.AddBlockTypeAttribute( "615506A5-DCFF-4842-A65E-C48E20FA8FCA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Appointment Entry Page", "AppointmentEntryPage", "", "Page to direct user to when the appointment needs to be rescheduled.", 0, @"", "92584952-6362-4A01-BAA8-90EF90DEC132" );
            RockMigrationHelper.AddBlockTypeAttribute( "615506A5-DCFF-4842-A65E-C48E20FA8FCA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Print Page", "PrintPage", "", "Page to use for printing appointment details.", 1, @"", "A5186930-B8A2-4AA0-AD3F-4891D9AD2D19" );

            RockMigrationHelper.AddBlock( "FBC943C6-52F4-4B47-A881-ADD53CF7BB1B", "", "615506A5-DCFF-4842-A65E-C48E20FA8FCA", "Appointment Detail", "Main", "", "", 0, "5908D84D-2988-40F5-B966-47CD8657B49B" );
            RockMigrationHelper.AddBlockAttributeValue( "5908D84D-2988-40F5-B966-47CD8657B49B", "92584952-6362-4A01-BAA8-90EF90DEC132", @"1b0052da-6138-4e53-947d-8bb5f6367aa6" ); // Appointment Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "5908D84D-2988-40F5-B966-47CD8657B49B", "A5186930-B8A2-4AA0-AD3F-4891D9AD2D19", @"5a8800d8-f73b-4015-9607-c6d2d10e91fe" ); // Print Page
        }

        private void AddAppointmentEntryPage()
        {
            RockMigrationHelper.AddPage( "E20405DB-F277-48BD-8455-85B38421A36B", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Add Appointment", "", "1B0052DA-6138-4E53-947D-8BB5F6367AA6", "fa fa-calendar-plus-o" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "1B0052DA-6138-4E53-947D-8BB5F6367AA6", "Appointment/Add/{PersonId}" );

            Sql( @"
    UPDATE [Page]
    SET [DisplayInNavWhen] = 2
    WHERE [Guid] = '1B0052DA-6138-4E53-947D-8BB5F6367AA6'
" );

            RockMigrationHelper.UpdateBlockType( "Appointment Entry", "Care Center block for adding new appointments.", "~/Plugins/org_willowcreek/CareCenter/AppointmentEntry.ascx", "org_willowcreek > CareCenter", "E83524BD-4994-4221-8E7D-2ED27057D741" );
            RockMigrationHelper.AddBlockTypeAttribute( "E83524BD-4994-4221-8E7D-2ED27057D741", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Service Area Caption", "ServiceAreaCaption", "", "Caption to use for 'Service Area' selection", 0, @"Service Area", "08EA50DF-B5AB-4A58-8DD6-5A265FC04DEA" );
            RockMigrationHelper.AddBlockTypeAttribute( "E83524BD-4994-4221-8E7D-2ED27057D741", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "Page to return to after appointment is scheduled", 1, @"", "1362DE1C-3635-46C1-9C95-627E598A684D" );
            RockMigrationHelper.AddBlockTypeAttribute( "E83524BD-4994-4221-8E7D-2ED27057D741", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Print Page", "PrintPage", "", "Page to use for printing appointment details.", 2, @"", "4C9BD369-9C84-40B0-A823-0B36BD52BE7B" );

            RockMigrationHelper.AddBlock( "1B0052DA-6138-4E53-947D-8BB5F6367AA6", "", "E83524BD-4994-4221-8E7D-2ED27057D741", "Appointment Entry", "Main", "", "", 0, "E0C48990-E3F9-4BD6-A284-EEC10FF8060D" );
            RockMigrationHelper.AddBlockAttributeValue( "E0C48990-E3F9-4BD6-A284-EEC10FF8060D", "08EA50DF-B5AB-4A58-8DD6-5A265FC04DEA", @"Service Area" ); // Service Area Caption
            RockMigrationHelper.AddBlockAttributeValue( "E0C48990-E3F9-4BD6-A284-EEC10FF8060D", "1362DE1C-3635-46C1-9C95-627E598A684D", @"fd92fe87-d749-40f6-8161-77cfd8431afb" ); // Home Page
            RockMigrationHelper.AddBlockAttributeValue( "E0C48990-E3F9-4BD6-A284-EEC10FF8060D", "4C9BD369-9C84-40B0-A823-0B36BD52BE7B", @"5a8800d8-f73b-4015-9607-c6d2d10e91fe" ); // Print Page

            AddAppointmentPrintPage();
        }

        private void AddAppointmentPrintPage()
        {
            RockMigrationHelper.AddPage( "1B0052DA-6138-4E53-947D-8BB5F6367AA6", "08C060A1-021D-4678-8C96-28D5268B1B76", "Print Appointment", "", "5A8800D8-F73B-4015-9607-C6D2D10E91FE", "" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Appointment Lava", "Care Center block for printing appointment details.", "~/Plugins/org_willowcreek/CareCenter/AppointmentLava.ascx", "org_willowcreek > CareCenter", "4A2284D9-A33C-4FFD-85C6-8E14FA305398" );
            RockMigrationHelper.AddBlockTypeAttribute( "4A2284D9-A33C-4FFD-85C6-8E14FA305398", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The Lava template to use for the appointment.", 0, @"
<style>
    .food-box {
        float: right;
        width: 200px;
        height: 40px;
        margin: 5px;
        border: 1px solid rgba(0, 0, 0, .2);
    }
</style>

<div class='clearfix'>
    <div class='pull-right'>
        <a href='#' class='btn btn-primary hidden-print' onClick='window.print();'><i class='fa fa-print'></i> Print</a> 
    </div>
</div>

<div>
    <div class='row margin-b-xl'>
        <div class='col-md-6'>
            <div class='pull-left'>
                <img src='/Themes/CareCenter/Assets/Images/logo.png' width='300px' />
            </div>
        </div>
        <div class='col-md-6 text-right'>
        </div>
    </div>

    <h3>Guest Appointment</h3>
    <hr>
    {{ Appointment.PersonAlias.Person.FullName }},<br/><br/>
    You have a <strong>{{ Appointment.TimeSlot.ServiceArea.Name }}</strong> appointment scheduled with
    the Willow Creek Care Center on <strong>{{ Appointment.AppointmentDate| Date:'dddd, MMMM d, yyyy' }}</strong>
    at <strong>{{ Appointment.TimeSlot.ScheduleTitle | Date:'hh:mm tt' }}</strong>.<br/>
</div>
", "48B9B04D-8A42-458D-970E-3D9315C0E5E7" );
            RockMigrationHelper.AddBlockTypeAttribute( "4A2284D9-A33C-4FFD-85C6-8E14FA305398", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Shows the merge fields available for the Lava", 1, @"False", "41E22DB2-FA29-4CB9-8115-D932AD38C82E" );

            RockMigrationHelper.AddBlock( "5A8800D8-F73B-4015-9607-C6D2D10E91FE", "", "4A2284D9-A33C-4FFD-85C6-8E14FA305398", "Appointment Lava", "Main", "", "", 0, "2562BD93-0B3F-4C93-B46B-4489B669293C" );
        }

        #endregion

        #region Dashboard Pages

        private void AddDashboardsPage()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Dashboards", "", "046C8279-360F-4EDE-A39D-F8170EF920B6", "fa fa-tachometer" );

            RockMigrationHelper.AddBlock( "046C8279-360F-4EDE-A39D-F8170EF920B6", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 1, "CA251557-F34E-4334-B230-CAD07C5D7790" );
            RockMigrationHelper.AddBlockAttributeValue( "CA251557-F34E-4334-B230-CAD07C5D7790", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "CA251557-F34E-4334-B230-CAD07C5D7790", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "CA251557-F34E-4334-B230-CAD07C5D7790", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "CA251557-F34E-4334-B230-CAD07C5D7790", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "CA251557-F34E-4334-B230-CAD07C5D7790", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );
            RockMigrationHelper.AddBlockAttributeValue( "CA251557-F34E-4334-B230-CAD07C5D7790", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "CA251557-F34E-4334-B230-CAD07C5D7790", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "CA251557-F34E-4334-B230-CAD07C5D7790", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            // Add Dashboard Block Type (used by all child pages)
            RockMigrationHelper.UpdateBlockType( "Dashboard", "Care Center Service Area Dashboard.", "~/Plugins/org_willowcreek/CareCenter/Dashboard.ascx", "org_willowcreek > CareCenter", "F7558444-D7D1-4774-9400-B41435913BCC" );
            RockMigrationHelper.AddBlockTypeAttribute( "F7558444-D7D1-4774-9400-B41435913BCC", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Service Areas", "ServiceAreas", "", "The service areas to display", 0, @"", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C" );
            RockMigrationHelper.AddBlockTypeAttribute( "F7558444-D7D1-4774-9400-B41435913BCC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Status(es)", "WorkflowStatus", "", "A comma-delimited list of workflow status to include (in addition to 'Pending')", 1, @"Waiting,In Progress", "3C876DFF-1D95-46C1-889B-F01DBA143B03" );
            RockMigrationHelper.AddBlockTypeAttribute( "F7558444-D7D1-4774-9400-B41435913BCC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "", "Page to direct user to when they click a service.", 2, @"", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E" );
            RockMigrationHelper.AddBlockTypeAttribute( "F7558444-D7D1-4774-9400-B41435913BCC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", 3, @"", "79F3AD4F-86F6-4334-A2D8-135D2477633C" );

            // Add Assessment Dashboard Block Type (currently only used for transportation dashboard)
            RockMigrationHelper.UpdateBlockType( "Assessment Dashboard", "Care Center Assessment Dashboard.", "~/Plugins/org_willowcreek/CareCenter/AssessmentDashboard.ascx", "org_willowcreek > CareCenter", "6523C238-B39B-4065-BAFA-40A9E604F65E" );
            RockMigrationHelper.AddBlockTypeAttribute( "6523C238-B39B-4065-BAFA-40A9E604F65E", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Workflows", "Workflows", "", "The workflow types to display", 0, @"", "BB166DE4-F226-4DE1-B523-EF289CC172F9" );
            RockMigrationHelper.AddBlockTypeAttribute( "6523C238-B39B-4065-BAFA-40A9E604F65E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Status(es)", "WorkflowStatus", "", "A comma-delimited list of workflow status to include (in addition to 'Pending')", 1, @"Waiting,In Progress", "3EB2A4C0-41BC-40E6-841E-CD9C321986FA" );
            RockMigrationHelper.AddBlockTypeAttribute( "6523C238-B39B-4065-BAFA-40A9E604F65E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "", "Page to direct user to when they click a service.", 2, @"", "C325A05B-28C9-4B14-AB47-5084E649319B" );
            RockMigrationHelper.AddBlockTypeAttribute( "6523C238-B39B-4065-BAFA-40A9E604F65E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", 3, @"", "DF24965D-56AA-460B-B383-45350C4B2F32" );

            AddCareTeamDashboardPage();
            AddClothingDashboardPage();
            AddEmploymentCounselingDashboardPage();
            AddFinancialCounselingDashboardPage();
            AddFoodDashboardPage();
            AddLegalCounselingDashboardPage();
            AddResourceVisitDashboardPage();
            //AddTaxPrepCounselingDashboardPage();
            //AddResponsePastorDashboardPage();
            AddTransportationDashboardPage();

            AddDashboardWorkflowEntryPage();
        }

        private void AddFoodDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Food", "", "5EDFE11C-369F-4F01-9FA0-BAF119EA1AD8", "fa fa-cutlery" );

            RockMigrationHelper.AddBlock( "5EDFE11C-369F-4F01-9FA0-BAF119EA1AD8", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Food Dashboard", "Main", "", "", 0, "E30C1F8D-6975-49EF-AC50-C40168FBA201" );
            RockMigrationHelper.AddBlockAttributeValue( "E30C1F8D-6975-49EF-AC50-C40168FBA201", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"B8154500-F8B2-4D7E-8926-196BB6DB9A82,3DA2915D-6D74-47B3-8B8A-F1B6041F9546" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "E30C1F8D-6975-49EF-AC50-C40168FBA201", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "E30C1F8D-6975-49EF-AC50-C40168FBA201", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "E30C1F8D-6975-49EF-AC50-C40168FBA201", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddClothingDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Clothing", "", "99165E93-956A-4A2B-851B-A5F6C798CEB4", "fa fa-tag" ); // Site:Care Center

            RockMigrationHelper.AddBlock( "99165E93-956A-4A2B-851B-A5F6C798CEB4", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Clothing Dashboard", "Main", "", "", 0, "71684648-7987-47E8-84FD-96B459D2C766" );
            RockMigrationHelper.AddBlockAttributeValue( "71684648-7987-47E8-84FD-96B459D2C766", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"32B9C517-AC4A-4FFA-AD17-61967C1CA1F6,DC2F126F-EB6A-47D3-8F44-C473035B8C53" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "71684648-7987-47E8-84FD-96B459D2C766", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "71684648-7987-47E8-84FD-96B459D2C766", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "71684648-7987-47E8-84FD-96B459D2C766", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddCareTeamDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Care Team", "", "D4F8E01E-FC5E-4166-A595-E28B355AE430", "fa fa-heartbeat" );

            RockMigrationHelper.AddBlock( "D4F8E01E-FC5E-4166-A595-E28B355AE430", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Care Team Dashboard", "Main", "", "", 0, "4273E9EE-5CF4-4E75-BB2F-B4E7B6CE75D1" );
            RockMigrationHelper.AddBlockAttributeValue( "4273E9EE-5CF4-4E75-BB2F-B4E7B6CE75D1", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"203CD767-CD7C-4CDE-A57D-162EA4FD0008" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "4273E9EE-5CF4-4E75-BB2F-B4E7B6CE75D1", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "4273E9EE-5CF4-4E75-BB2F-B4E7B6CE75D1", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "4273E9EE-5CF4-4E75-BB2F-B4E7B6CE75D1", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddResourceVisitDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Resource Visit", "", "C8CA758B-62BC-462E-8666-DE161195AFC2", "fa fa-clipboard" ); // Site:Care Center

            RockMigrationHelper.AddBlock( "C8CA758B-62BC-462E-8666-DE161195AFC2", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Dashboard", "Main", "", "", 0, "075C1210-7E2C-410E-92A5-EAA89AAB5BF1" );
            RockMigrationHelper.AddBlockAttributeValue( "075C1210-7E2C-410E-92A5-EAA89AAB5BF1", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"60157D50-94C8-4747-96B5-F03307FE6F10" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "075C1210-7E2C-410E-92A5-EAA89AAB5BF1", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "075C1210-7E2C-410E-92A5-EAA89AAB5BF1", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "075C1210-7E2C-410E-92A5-EAA89AAB5BF1", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddEmploymentCounselingDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Employment Counseling", "", "D24C9A1F-EDF2-459F-8E81-4E96FF7948F1", "fa fa-handshake-o" );

            RockMigrationHelper.AddBlock( "D24C9A1F-EDF2-459F-8E81-4E96FF7948F1", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Employment Dashboard", "Main", "", "", 0, "FB7996E8-3F53-4FA8-8C24-946446595FFB" );
            RockMigrationHelper.AddBlockAttributeValue( "FB7996E8-3F53-4FA8-8C24-946446595FFB", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"C20E771F-AE2E-49C7-B31D-C0D63214C74E" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "FB7996E8-3F53-4FA8-8C24-946446595FFB", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "FB7996E8-3F53-4FA8-8C24-946446595FFB", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "FB7996E8-3F53-4FA8-8C24-946446595FFB", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddFinancialCounselingDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Financial Counseling", "", "5725CAE5-66A0-4BDE-BA40-F3FFD8D62E01", "fa fa-dollar" );

            RockMigrationHelper.AddBlock( "5725CAE5-66A0-4BDE-BA40-F3FFD8D62E01", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Financial Dashboard", "Main", "", "", 0, "E46B4BA5-3551-4D34-AE9A-8A34DB27C28F" );
            RockMigrationHelper.AddBlockAttributeValue( "E46B4BA5-3551-4D34-AE9A-8A34DB27C28F", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"64168E99-9C1D-48BC-AF7C-BF72E4A44928" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "E46B4BA5-3551-4D34-AE9A-8A34DB27C28F", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "E46B4BA5-3551-4D34-AE9A-8A34DB27C28F", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "E46B4BA5-3551-4D34-AE9A-8A34DB27C28F", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddLegalCounselingDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Legal Counseling", "", "2A511283-9BE8-4F6F-BD79-7FFC4924F250", "fa fa-legal" );

            RockMigrationHelper.AddBlock( "2A511283-9BE8-4F6F-BD79-7FFC4924F250", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Legal Dashboard", "Main", "", "", 0, "B5E120C9-4400-48D2-BBF4-EF5D8C7E40E4" );
            RockMigrationHelper.AddBlockAttributeValue( "B5E120C9-4400-48D2-BBF4-EF5D8C7E40E4", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"2050BF96-9F8C-471B-B6E1-4151A5E428F9,095E13E5-4F84-4D7E-B30D-5FE1AD69BD3E" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "B5E120C9-4400-48D2-BBF4-EF5D8C7E40E4", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,Paged,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "B5E120C9-4400-48D2-BBF4-EF5D8C7E40E4", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "B5E120C9-4400-48D2-BBF4-EF5D8C7E40E4", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddTaxPrepCounselingDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Tax Prep Counseling", "", "19944836-34BA-4607-B1E2-C59D20AE897B", "fa fa-clipboard" );

            RockMigrationHelper.AddBlock( "19944836-34BA-4607-B1E2-C59D20AE897B", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Tax Prep Dashboard", "Main", "", "", 0, "3A1CCD29-D6DE-4CE5-9D4B-5411BEE7A45F" );
            RockMigrationHelper.AddBlockAttributeValue( "3A1CCD29-D6DE-4CE5-9D4B-5411BEE7A45F", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"52E00962-B455-485B-ADD1-9BE5D67574D4" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "3A1CCD29-D6DE-4CE5-9D4B-5411BEE7A45F", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "3A1CCD29-D6DE-4CE5-9D4B-5411BEE7A45F", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "3A1CCD29-D6DE-4CE5-9D4B-5411BEE7A45F", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddResponsePastorDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Response Pastor", "", "2ACA9BAC-E18F-4273-9DF4-3D76DD4B9B4E", "fa fa-user" ); // Site:Care Center

            RockMigrationHelper.AddBlock( "2ACA9BAC-E18F-4273-9DF4-3D76DD4B9B4E", "", "F7558444-D7D1-4774-9400-B41435913BCC", "Dashboard", "Main", "", "", 0, "22BA8B53-B4D6-4EFB-989A-EE87C6E9B688" );
            RockMigrationHelper.AddBlockAttributeValue( "22BA8B53-B4D6-4EFB-989A-EE87C6E9B688", "A58F3D2C-A897-47BC-9E9B-A50AC2DBE40C", @"C8EA27AD-2931-4119-ACE8-3C0F86AD7E9A" ); // Service Areas
            RockMigrationHelper.AddBlockAttributeValue( "22BA8B53-B4D6-4EFB-989A-EE87C6E9B688", "3C876DFF-1D95-46C1-889B-F01DBA143B03", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "22BA8B53-B4D6-4EFB-989A-EE87C6E9B688", "FBB29DBF-A336-4A58-80E0-6ADC17B0E64E", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "22BA8B53-B4D6-4EFB-989A-EE87C6E9B688", "79F3AD4F-86F6-4334-A2D8-135D2477633C", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page
        }

        private void AddTransportationDashboardPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Transportation", "", "C03650FE-361D-4B0A-BC27-28A2F941A015", "fa fa-car" ); // Site:Care Center

            RockMigrationHelper.AddBlock( "C03650FE-361D-4B0A-BC27-28A2F941A015", "", "6523C238-B39B-4065-BAFA-40A9E604F65E", "Assessment Dashboard", "Main", "", "", 0, "084DC7DC-8FB9-4A34-9517-27DC264407DB" );
            RockMigrationHelper.AddBlockAttributeValue( "084DC7DC-8FB9-4A34-9517-27DC264407DB", "C325A05B-28C9-4B14-AB47-5084E649319B", @"4dc59bfc-ad37-48ba-8092-504bcdb29bde" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "084DC7DC-8FB9-4A34-9517-27DC264407DB", "DF24965D-56AA-460B-B383-45350C4B2F32", @"b3322488-717f-459f-b63f-813c68a78db9,54a6c106-9bd5-4ea9-a3ce-11c22b08f12f" ); // Person Profile Page
            RockMigrationHelper.AddBlockAttributeValue( "084DC7DC-8FB9-4A34-9517-27DC264407DB", "3EB2A4C0-41BC-40E6-841E-CD9C321986FA", @"Waiting,In Progress" ); // Workflow Status(es)
            RockMigrationHelper.AddBlockAttributeValue( "084DC7DC-8FB9-4A34-9517-27DC264407DB", "BB166DE4-F226-4DE1-B523-EF289CC172F9", @"DF79DEAD-0551-4970-9DA9-75C5AD74956D,77E52E29-C6F9-4C8B-AAD4-E6F383B01A88,44AB1CF0-B4FE-4D7F-9BF3-F3D7BC90B4D1,FC49EB7C-5A1C-407C-9B12-C05B4987CEB8" ); // Workflows
        }

        private void AddDashboardWorkflowEntryPage()
        {
            RockMigrationHelper.AddPage( "046C8279-360F-4EDE-A39D-F8170EF920B6", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Details", "", "4DC59BFC-AD37-48BA-8092-504BCDB29BDE", "" ); // Site:Care Center

            Sql( @"
    UPDATE [Page]
    SET 
        [DisplayInNavWhen] = 2,
        [BreadCrumbDisplayName] = 0
    WHERE [Guid] = '4DC59BFC-AD37-48BA-8092-504BCDB29BDE'
" );

            RockMigrationHelper.UpdateBlockType( "Dashboard Workflow", "Block to display button for returning to dashboard from the workflow form page.", "~/Plugins/org_willowcreek/CareCenter/DashboardWorkflow.ascx", "org_willowcreek > CareCenter", "C25C857C-13F3-4D23-8CBB-DC8A32C8F350" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C25C857C-13F3-4D23-8CBB-DC8A32C8F350", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Workflow Type Dashboard Page", "WorkflowTypeDashboardPage", "", "The Workflow Types and their cooresponding Page Ids", 0, @"", "3C5C8EEC-60BE-4207-A012-AFA814429528" );

            RockMigrationHelper.AddBlock( "4DC59BFC-AD37-48BA-8092-504BCDB29BDE", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "75BC9F51-91E7-4BB5-898D-CC58CC6BE479" );

            RockMigrationHelper.AddBlock( "4DC59BFC-AD37-48BA-8092-504BCDB29BDE", "", "C25C857C-13F3-4D23-8CBB-DC8A32C8F350", "Dashboard Workflow", "Main", "", "", 1, "A9695AF5-EEBC-4340-A373-EABF574909CC" );
            RockMigrationHelper.AddBlockAttributeValue( "A9695AF5-EEBC-4340-A373-EABF574909CC", "3C5C8EEC-60BE-4207-A012-AFA814429528", @"23^477|25^478|24^479|26^480|27^481|29^482|28^477|30^478" ); // Workflow Type Dashboard Page

            Sql( @"
    DECLARE @BreadVisitWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '2A5ACB4A-69DE-4D16-BDA3-C7187297D3B9' )
    DECLARE @CareTeamWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = 'CFA47B9F-1380-4705-B50B-F0B122B386A3' )
    DECLARE @ClothingVisitWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '99830D9E-E8AF-4757-A84E-2D4C4EC5BE09' )
    DECLARE @EmploymentWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '9196C4D9-A820-4217-9341-44B2C0084F74' )
    DECLARE @FinancialCoachingWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '2A76CDB2-F330-4C9B-A1E8-24308041CB03' )
    DECLARE @FoodVisitWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = 'BCE8C80C-81FF-4580-9000-A09A4B8920E3' )
    DECLARE @LegalWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '988FDF54-86D4-4CC3-BB4F-DE25A9D494B2' )
    DECLARE @LegalImmigrationWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '6B37C278-5C83-49DF-86B7-3F4C51AB52CD' )
    DECLARE @LimitedClothingVisitWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '46B04B7C-699C-4513-804E-BA54A11B2AF9' )
    DECLARE @ResourceWorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = 'EA47030D-D539-4509-BA46-A57ED61D61E4' )

    DECLARE @FoodPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '5EDFE11C-369F-4F01-9FA0-BAF119EA1AD8' )
    DECLARE @ClothingPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '99165E93-956A-4A2B-851B-A5F6C798CEB4' )
    DECLARE @CareTeamPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'D4F8E01E-FC5E-4166-A595-E28B355AE430' )
    DECLARE @ResourceVisitPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'C8CA758B-62BC-462E-8666-DE161195AFC2' )
    DECLARE @EmploymentCounselingPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'D24C9A1F-EDF2-459F-8E81-4E96FF7948F1' )
    DECLARE @FinancialCounselingPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '5725CAE5-66A0-4BDE-BA40-F3FFD8D62E01' )
    DECLARE @LegalCounselingPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '2A511283-9BE8-4F6F-BD79-7FFC4924F250' )

    DECLARE @BlockId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'A9695AF5-EEBC-4340-A373-EABF574909CC' )
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '3C5C8EEC-60BE-4207-A012-AFA814429528' )

    UPDATE [AttributeValue] SET [Value] = 
	    CAST( @BreadVisitWorkflowTypeId as varchar(5) ) + '^' + CAST( @FoodPageId as varchar(5) ) + '|' +
	    CAST( @CareTeamWorkflowTypeId as varchar(5) ) + '^' + CAST( @CareTeamPageId as varchar(5) ) + '|' +
	    CAST( @ClothingVisitWorkflowTypeId as varchar(5) ) + '^' + CAST( @ClothingPageId as varchar(5) ) + '|' +
	    CAST( @EmploymentWorkflowTypeId as varchar(5) ) + '^' + CAST( @EmploymentCounselingPageId as varchar(5) ) + '|' +
	    CAST( @FinancialCoachingWorkflowTypeId as varchar(5) ) + '^' + CAST( @FinancialCounselingPageId as varchar(5) ) + '|' +
	    CAST( @FoodVisitWorkflowTypeId as varchar(5) ) + '^' + CAST( @FoodPageId as varchar(5) ) + '|' +
	    CAST( @LegalWorkflowTypeId as varchar(5) ) + '^' + CAST( @LegalCounselingPageId as varchar(5) ) + '|' +
	    CAST( @LegalImmigrationWorkflowTypeId as varchar(5) ) + '^' + CAST( @LegalCounselingPageId as varchar(5) ) + '|' +
	    CAST( @LimitedClothingVisitWorkflowTypeId as varchar(5) ) + '^' + CAST( @ClothingPageId as varchar(5) ) + '|' +
	    CAST( @ResourceWorkflowTypeId as varchar(5) ) + '^' + CAST( @ResourceVisitPageId as varchar(5) ) 
    WHERE [AttributeId] = @AttributeId
    AND [EntityId] = @BlockId
" );
        }

        #endregion

        #region Visit Pages

        private void AddVisitsPage()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Visits", "", "1033A239-5626-4DC6-997F-180C951C6F3D", "fa fa-users" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Visit List", "Care Center block for viewing list of visits.", "~/Plugins/org_willowcreek/CareCenter/VisitList.ascx", "org_willowcreek > CareCenter", "D85EB94A-448A-4BF5-B7A4-2A0673AF1819" );

            RockMigrationHelper.AddBlockTypeAttribute( "D85EB94A-448A-4BF5-B7A4-2A0673AF1819", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page for displaying details of a visit", 0, @"", "CEEC8FB9-4CAE-4DD9-9C92-D744B0F9F649" );
            RockMigrationHelper.AddBlockTypeAttribute( "D85EB94A-448A-4BF5-B7A4-2A0673AF1819", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Passport Page", "PassportPage", "", "The Page for viewing and printing passports", 1, @"", "420493CB-2FEB-43CA-8499-C880D6BCC05D" );
            RockMigrationHelper.AddBlockTypeAttribute( "D85EB94A-448A-4BF5-B7A4-2A0673AF1819", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", 2, @"", "EE52906F-0227-4C31-A7C2-C2B991E903A0" );
            RockMigrationHelper.AddBlockTypeAttribute( "D85EB94A-448A-4BF5-B7A4-2A0673AF1819", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Passport Workflows", "PassportWorkflows", "", "The workflow types that need a passport", 3, @"", "E3232DDD-4D24-4FED-8F84-0A0A812D458F" );

            RockMigrationHelper.AddBlock( "1033A239-5626-4DC6-997F-180C951C6F3D", "", "D85EB94A-448A-4BF5-B7A4-2A0673AF1819", "Visit List", "Main", "", "", 0, "DE4419AA-51CA-4F5D-817E-BB8F90CB4296" );
            RockMigrationHelper.AddBlockAttributeValue( "DE4419AA-51CA-4F5D-817E-BB8F90CB4296", "CEEC8FB9-4CAE-4DD9-9C92-D744B0F9F649", @"2a06d3e9-ef49-4d0f-b3de-d3845f1abb07" ); // Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "DE4419AA-51CA-4F5D-817E-BB8F90CB4296", "420493CB-2FEB-43CA-8499-C880D6BCC05D", @"b8d78644-3473-42f9-8973-904dc63ce216" ); // Passport Page
            RockMigrationHelper.AddBlockAttributeValue( "DE4419AA-51CA-4F5D-817E-BB8F90CB4296", "E3232DDD-4D24-4FED-8F84-0A0A812D458F", @"99830d9e-e8af-4757-a84e-2d4c4ec5be09,46b04b7c-699c-4513-804e-ba54a11b2af9,bce8c80c-81ff-4580-9000-a09a4b8920e3,2a5acb4a-69de-4d16-bda3-c7187297d3b9" ); // Passport Workflows
            RockMigrationHelper.AddBlockAttributeValue( "DE4419AA-51CA-4F5D-817E-BB8F90CB4296", "EE52906F-0227-4C31-A7C2-C2B991E903A0", @"b3322488-717f-459f-b63f-813c68a78db9,13f1e18c-2fe7-4d84-ae25-6dfd3ec4fd4a" ); // Person Profile Page

            AddVisitDetailsPage();
        }

        private void AddVisitDetailsPage()
        {
            RockMigrationHelper.AddPage( "1033A239-5626-4DC6-997F-180C951C6F3D", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Visit Details", "", "2A06D3E9-EF49-4D0F-B3DE-D3845F1ABB07", "" ); // Site:Care Center
            RockMigrationHelper.UpdatePageContext( "2A06D3E9-EF49-4D0F-B3DE-D3845F1ABB07", "org.willowcreek.CareCenter.Model.Visit", "VisitId", "34F6251F-798A-4BEC-A6CF-E82073648721" );

            RockMigrationHelper.UpdateBlockType( "Visit Detail", "Care Center block for viewing/editing details of a visit.", "~/Plugins/org_willowcreek/CareCenter/VisitDetail.ascx", "org_willowcreek > CareCenter", "C947F564-16BE-497D-8517-8AB35CF02C44" );
            RockMigrationHelper.AddBlockTypeAttribute( "C947F564-16BE-497D-8517-8AB35CF02C44", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "", "Page to direct user to when they click a service.", 0, @"", "282B98C8-80F4-4294-A19F-4706F473466F" );
            RockMigrationHelper.AddBlockTypeAttribute( "C947F564-16BE-497D-8517-8AB35CF02C44", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Intake Page", "IntakePage", "", "Page to direct user to when they add a new service.", 1, @"", "392C38D1-7183-4896-9FC1-65B1655E0E0D" );
            RockMigrationHelper.AddBlockTypeAttribute( "C947F564-16BE-497D-8517-8AB35CF02C44", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Passport Page", "PassportPage", "", "Page to view and print a Passport.", 2, @"", "52D0160E-0F7B-45ED-8E9C-8EE3BBFC7353" );

            RockMigrationHelper.AddBlock( "2A06D3E9-EF49-4D0F-B3DE-D3845F1ABB07", "", "C947F564-16BE-497D-8517-8AB35CF02C44", "Visit Detail", "Main", "", "", 0, "03F5C873-1DC7-49CB-A4B3-B3D3007184E9" );
            RockMigrationHelper.AddBlockAttributeValue( "03F5C873-1DC7-49CB-A4B3-B3D3007184E9", "282B98C8-80F4-4294-A19F-4706F473466F", @"9a41d5f3-1b32-4b6e-8c31-a3d615caf089" ); // Workflow Entry Page
            RockMigrationHelper.AddBlockAttributeValue( "03F5C873-1DC7-49CB-A4B3-B3D3007184E9", "392C38D1-7183-4896-9FC1-65B1655E0E0D", @"54691ba5-b00c-490a-b7c1-4c86eb2ba5a9" ); // Intake Page
            RockMigrationHelper.AddBlockAttributeValue( "03F5C873-1DC7-49CB-A4B3-B3D3007184E9", "52D0160E-0F7B-45ED-8E9C-8EE3BBFC7353", @"b8d78644-3473-42f9-8973-904dc63ce216" ); // Passport Page

            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.Model.Visit", "76E2EC72-F496-424E-91F1-D6E1DF408BF9", true, true );
            Sql( @"
    DECLARE @NoteEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '76E2EC72-F496-424E-91F1-D6E1DF408BF9' )
    IF @NoteEntityTypeId IS NOT NULL
    BEGIN
	    INSERT INTO [NoteType]
		    ([IsSystem], [EntityTypeId], [Name], [Guid], [UserSelectable], [IconCssClass], [EntityTypeQualifierColumn], [EntityTypeQualifierValue])
	    VALUES
		    (1, @NoteEntityTypeId, 'Visit Note', 'F165EBA0-408B-4A1F-9B50-EE22F9B64290', 1, 'fa fa-comment', '', '')
    END
" );

            RockMigrationHelper.AddBlock( "2A06D3E9-EF49-4D0F-B3DE-D3845F1ABB07", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 1, "B6DBDCEA-9251-45D1-B95A-FFBF9687B552" );
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"76e2ec72-f496-424e-91f1-d6e1df408bf9" ); // Entity Type
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" ); // Show Private Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" ); // Show Security Button
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "20243A98-4802-48E2-AF61-83956056AC65", @"True" ); // Show Alert Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" ); // Heading
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-sticky-note-o" ); // Heading Icon CSS Class
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" ); // Note Term
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" ); // Display Type
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" ); // Use Person Icon
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" ); // Allow Anonymous
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" ); // Add Always Visible
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" ); // Display Order
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" ); // Allow Backdated Notes
            RockMigrationHelper.AddBlockAttributeValue( "B6DBDCEA-9251-45D1-B95A-FFBF9687B552", "C9735D5E-15F8-45A2-B5D8-97ED6137AD40", @"F165EBA0-408B-4A1F-9B50-EE22F9B64290" ); // Note Types

            AddVisitDetailsWorkflowEntryPage();
            AddPassportPage();
        }

        private void AddVisitDetailsWorkflowEntryPage()
        {
            RockMigrationHelper.AddPage( "2A06D3E9-EF49-4D0F-B3DE-D3845F1ABB07", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Workflow Entry", "", "9A41D5F3-1B32-4B6E-8C31-A3D615CAF089", "" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "9A41D5F3-1B32-4B6E-8C31-A3D615CAF089", "VisitWorkflow/{WorkflowTypeId}/{WorkflowId}" );
            RockMigrationHelper.AddPageRoute( "9A41D5F3-1B32-4B6E-8C31-A3D615CAF089", "VisitWorkflow/{WorkflowTypeId}" );

            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0
	WHERE [Guid] = '9A41D5F3-1B32-4B6E-8C31-A3D615CAF089'
" );

            RockMigrationHelper.AddBlock( "9A41D5F3-1B32-4B6E-8C31-A3D615CAF089", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "484A70C4-8749-4AC4-8EF9-A417345A7922" );
        }

        private void AddPassportPage()
        {
            RockMigrationHelper.AddPage( "2A06D3E9-EF49-4D0F-B3DE-D3845F1ABB07", "08C060A1-021D-4678-8C96-28D5268B1B76", "Passport", "", "B8D78644-3473-42F9-8973-904DC63CE216", "" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Passport Lava", "Care Center block for printing the Passport.", "~/Plugins/org_willowcreek/CareCenter/PassportLava.ascx", "org_willowcreek > CareCenter", "5617B994-A2E2-4C06-AFB1-C63D32BCB99F" );
            RockMigrationHelper.AddBlockTypeAttribute( "5617B994-A2E2-4C06-AFB1-C63D32BCB99F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The Lava template to use for the passport.", 0, @"
<style>
    .food-box {
        float: right;
        width: 200px;
        height: 40px;
        margin: 5px;
        border: 1px solid rgba(0, 0, 0, .2);
    }
</style>

<div class='clearfix'>
    <div class='pull-right'>
        <a href='#' class='btn btn-primary hidden-print' onClick='window.print();'><i class='fa fa-print'></i> Print Passport(s)</a> 
    </div>
</div>

{% for visit in Visits %}

    <div style='page-break-after:always'>
        <div class='row margin-b-xl'>
            <div class='col-md-6'>
                <div class='pull-left'>
                    <img src='/Themes/CareCenter/Assets/Images/logo.png' width='300px' />
                </div>
            </div>
            <div class='col-md-6 text-right'>
                <h3>Guest Passport</h3>
                Intake Day: <strong>{{ visit.VisitDate | Date:'M/d/yyyy' }} @ {{ visit.VisitDate | Date:'hh:mm:ss tt' }}</strong><br/>
                Pager #: <strong><u>{{ visit.PagerId }}</u></strong><br/>
                {% if visit.IsFirstVisit %}
                    <span class='label label-info'>First Visit</span>
                {% endif %}
            </div>
        </div>

        <h3>{{ visit.PersonAlias.Person.FullName }}</h3>
        <hr>
        <p>Primary Language: <strong>{{ visit.PersonAlias.Person | Attribute:'PreferredLanguage' }}</strong></p>

        {% assign groupMember = visit.PersonAlias.Person | Groups:""10"" | First %}
        {% assign familySize = groupMember.Group.Members | Size %}
        {% assign children = visit.PersonAlias.Person | Children %}

        {% assign wfs = visit.Workflows | Where:'WorkflowTypeId',39 %}
        {% if wfs != empty %}

            {% assign diapers = false %}
            {% assign birthday = false %}
            {% assign diaperAgeWeeks = 'Global' | Attribute:'DiaperAge' %}
            {% assign birthdayDaySpan = 'Global' | Attribute:'BirthdayCakeAvailableTimespan' %}

            {% for familyMember in groupMember.Group.Members %}
                {% assign birthdate = familyMember.Person.BirthDate %} 
                {% if birthdate %}

                    {% assign daysOld = birthdate | DateDiff:'Now','d' %}
                    {% assign weeksOld = daysOld | DividedBy:7 %}
                    {% if weeksOld <= diaperAgeWeeks %}
                        {% assign diapers = true %}
                    {% endif %}

                    {% assign daysToBirthday = familyMember.Person.DaysToBirthday %}
                    {% assign daysSinceBirthday = 365 | Minus:daysToBirthday %}
                    {% if daysToBirthday <= birthdayDaySpan or daysSinceBirthday <= birthdayDaySpan %}
                        {% assign birthday = true %}
                    {% endif %}

                {% endif %}
            {% endfor %}

            {% assign wf = wfs | First %}
            <hr>
            <h3>{{ wf.WorkflowType.Name }}:</h3>
            <div class='row'>
                <div class='col-sm-4'>
                    Family Size: <strong>{{ familySize }}</strong><br/>
                    Diapers: <strong>{% if diapers == true %}Yes{% else %}No{% endif %}</strong>
                </div>
                <div class='col-sm-4'>
                    Car Make/Model: <strong>{{ wf | Attribute:'CarMakeModel' }}</strong><br/>
                    In Car With: <strong>{{ wf | Attribute:'InCarWith' }}</strong><br/>
                </div>
                <div class='col-sm-4 clearfix'>
                    {% if birthday == true %}<i class='fa fa-birthday-cake fa-3x'></i>{% endif %}
                    <div class='food-box btn-{% if familySize > 7 %}warning{% else %}{% if familySize > 3 %}danger{% else %}info{% endif %}{% endif %}'></div>
                </div>
            </div>

        {% endif %}

        {% assign wfs = visit.Workflows | Where:'WorkflowTypeId',42 %}
        {% if wfs != empty %}
            {% assign wf = wfs | First %}
            <hr>
            <h3>{{ wf.WorkflowType.Name }}:</h3>
        {% endif %}

        {% assign wfs = visit.Workflows | Where:'WorkflowTypeId',35 %}
        {% if wfs != empty %}
            {% assign wf = wfs | First %}
            <hr>
            <h3>{{ wf.WorkflowType.Name }}:</h3>
            <table style='border-collapse:collapse;'>
                <thead>
                    <tr>
                        <th style='border:1px solid #cccccc;padding:5px'>Child's Name</th>
                        <th style='border:1px solid #cccccc;padding:5px'>Gender</th>
                        <th style='border:1px solid #cccccc;padding:5px'>Age</th>
                        <th style='border:1px solid #cccccc;padding:5px'>Coat</th>
                   </tr>
                </thead>
                <tbody>
                {% for child in children %}
                    {% assign coatValid = 'Yes' %}
                    {% assign coatDate = child | Attribute:'CoatReception','RawValue' %}
                    {% if coatDate and coatDate != empty and coatDate != '' %}
                        {% if coatDate | DateDiff:'Now','Y' < 1 %}
                            {% assign coatValid = 'No' %}
                        {% endif %}
                    {% endif %}
                    <tr>
                        <td style='border:1px solid #cccccc;padding:5px'>{{ child.FullName }}</td>
                        <td style='border:1px solid #cccccc;padding:5px'>{{ child.Gender }}</td>
                        <td style='border:1px solid #cccccc;padding:5px'>{{ child.Age }}</td>
                        <td style='border:1px solid #cccccc;padding:5px'>{{ coatValid }}</td>
                   </tr>
                {% endfor %}
                </tbody>
            </table>            
        {% endif %}

        {% assign wfs = visit.Workflows | Where:'WorkflowTypeId',38 %}
        {% if wfs != empty %}
            {% assign wf = wfs | First %}
            <hr>
            <h3>{{ wf.WorkflowType.Name }}:</h3>
            <p>This is a limited clothing visit. This guest is allowed up to 10 clothing items.</p>
        {% endif %}

    </div>

{% endfor %}
", "166EA9C0-04BE-4DAA-A8D2-A21A48F73C64");
            RockMigrationHelper.AddBlockTypeAttribute("5617B994-A2E2-4C06-AFB1-C63D32BCB99F","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Shows the merge fields available for the Lava",1,@"False","4D7B7AD4-0EC5-4985-9DDE-921BEB47CE1A");

            RockMigrationHelper.AddBlock( "B8D78644-3473-42F9-8973-904DC63CE216", "", "5617B994-A2E2-4C06-AFB1-C63D32BCB99F", "Passport Lava", "Main", "", "", 0, "DD83DB12-F2B9-4BDF-B0A3-30A793313861" );

        }

        #endregion

        #region Assessment Pages

        private void AddAssessmentsPage()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Assessments", "", "799BDFE5-EF56-48BA-80B7-843EA2AD5E1B", "fa fa-check-square-o" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "799BDFE5-EF56-48BA-80B7-843EA2AD5E1B", "Assessments" );

            RockMigrationHelper.UpdateBlockType( "Assessment List", "Care Center block for viewing list of assessments.", "~/Plugins/org_willowcreek/CareCenter/AssessmentList.ascx", "org_willowcreek > CareCenter", "B7E61C30-273E-41FD-97BE-FD19F43087D2" );
            RockMigrationHelper.AddBlockTypeAttribute( "B7E61C30-273E-41FD-97BE-FD19F43087D2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page for displaying details of an assessment", 0, @"", "28FE5BC1-4A6E-445B-987B-E6F8A083CB98" );
            RockMigrationHelper.AddBlockTypeAttribute( "B7E61C30-273E-41FD-97BE-FD19F43087D2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", 2, @"", "1083387A-14B3-4011-9FB4-F0806A8C22FC" );

            RockMigrationHelper.AddBlock( "799BDFE5-EF56-48BA-80B7-843EA2AD5E1B", "", "B7E61C30-273E-41FD-97BE-FD19F43087D2", "Assessment List", "Main", "", "", 0, "DB0168F7-BBDC-4F2A-A223-39E1B538C248" );
            RockMigrationHelper.AddBlockAttributeValue( "DB0168F7-BBDC-4F2A-A223-39E1B538C248", "1083387A-14B3-4011-9FB4-F0806A8C22FC", @"b3322488-717f-459f-b63f-813c68a78db9,975416e9-fe0c-4623-a5d4-671abc77fc89" ); // Person Profile Page
            RockMigrationHelper.AddBlockAttributeValue( "DB0168F7-BBDC-4F2A-A223-39E1B538C248", "28FE5BC1-4A6E-445B-987B-E6F8A083CB98", @"2eb1e4f1-b4de-426f-9372-3dfb3d6cd22f" ); // Detail Page

            AddAssessmentDetailPage();
        }

        private void AddAssessmentDetailPage()
        {
            RockMigrationHelper.AddPage( "799BDFE5-EF56-48BA-80B7-843EA2AD5E1B", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Assessment", "", "2EB1E4F1-B4DE-426F-9372-3DFB3D6CD22F", "" ); // Site:Care Center
            RockMigrationHelper.UpdatePageContext( "2EB1E4F1-B4DE-426F-9372-3DFB3D6CD22F", "org.willowcreek.CareCenter.Model.Assessment", "AssessmentId", "739EED60-51D3-4DDF-9A97-6FA4ACA2F5F9" );

            RockMigrationHelper.UpdateBlockType( "Assessment Detail", "Care Center block for viewing/editing details of a assessment.", "~/Plugins/org_willowcreek/CareCenter/AssessmentDetail.ascx", "org_willowcreek > CareCenter", "ADBFAAA4-2DC4-4DB7-8D54-B9CAF791C67F" );
            RockMigrationHelper.AddBlockTypeAttribute( "ADBFAAA4-2DC4-4DB7-8D54-B9CAF791C67F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "", "Page to direct user to when they click a service.", 0, @"", "8976D74E-529F-4ABD-91B7-40824BB5CF1A" );

            RockMigrationHelper.AddBlock( "2EB1E4F1-B4DE-426F-9372-3DFB3D6CD22F", "", "ADBFAAA4-2DC4-4DB7-8D54-B9CAF791C67F", "Assessment Detail", "Main", "", "", 0, "5E77D18C-4204-461B-B274-961D825C8120" );
            RockMigrationHelper.AddBlock( "2EB1E4F1-B4DE-426F-9372-3DFB3D6CD22F", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 1, "CAF7A2A4-3C79-4419-9748-EBABF04A3D03" );

            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.Model.Assessment", "9D3E8186-6B1C-4339-9E8F-76FBECFDF5C5", true, true );
            Sql( @"
    DECLARE @NoteEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '9D3E8186-6B1C-4339-9E8F-76FBECFDF5C5' )
    IF @NoteEntityTypeId IS NOT NULL
    BEGIN
	    INSERT INTO [NoteType]
		    ([IsSystem], [EntityTypeId], [Name], [Guid], [UserSelectable], [IconCssClass], [EntityTypeQualifierColumn], [EntityTypeQualifierValue])
	    VALUES
		    (1, @NoteEntityTypeId, 'Assessment Note', 'B032BD89-92CC-4E1C-9575-426C3A7DD175', 1, 'fa fa-comment', '', '')
    END
" );

            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"9d3e8186-6b1c-4339-9e8f-76fbecfdf5c5" ); // Entity Type
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" ); // Show Private Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" ); // Show Security Button
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "20243A98-4802-48E2-AF61-83956056AC65", @"True" ); // Show Alert Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" ); // Heading
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-sticky-note-o" ); // Heading Icon CSS Class
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" ); // Note Term
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" ); // Display Type
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" ); // Use Person Icon
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" ); // Allow Anonymous
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" ); // Add Always Visible
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" ); // Display Order
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" ); // Allow Backdated Notes
            RockMigrationHelper.AddBlockAttributeValue( "CAF7A2A4-3C79-4419-9748-EBABF04A3D03", "03A0974C-7BC6-4531-B31A-381D74F2CD76", @"B032BD89-92CC-4E1C-9575-426C3A7DD175" ); // Note Types

            AddAssessmentWorkflowEntryPage();
        }

        private void AddAssessmentWorkflowEntryPage()
        {
            RockMigrationHelper.AddPage( "2EB1E4F1-B4DE-426F-9372-3DFB3D6CD22F", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Workflow Entry", "", "EBA4D9C8-E38C-4135-B885-4A971018E865", "" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "EBA4D9C8-E38C-4135-B885-4A971018E865", "AssessmentWorkflow/{WorkflowTypeId}/{WorkflowId}" );
            RockMigrationHelper.AddPageRoute( "EBA4D9C8-E38C-4135-B885-4A971018E865", "AssessmentWorkflow/{WorkflowTypeId}" );

            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0
	WHERE [Guid] = 'EBA4D9C8-E38C-4135-B885-4A971018E865'
" );
            RockMigrationHelper.AddBlock( "EBA4D9C8-E38C-4135-B885-4A971018E865", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "4877BB59-10CD-426C-84D4-1639B7F919E3" );
        }

        #endregion

        #region Care Pastor Pages

        private void AddCarePastorPage()
        {
            // Page: Care Pastor
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Care Pastor", "", "9F690F70-1FF2-4BBE-AF6C-3DD60526C321", "fa fa-medkit" ); // Site:Care Center
        }

        #endregion

        #region Resources Pages

        private void AddResourcesPage()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Resources", "", "E3B2D931-4D31-459B-87AB-265FE194D7E0", "fa fa-book" );
        }

        #endregion

        #region Event Registration Pages

        private void AddEventRegistrationPages()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Event Registration", "", "137E18FB-BE84-4B1C-B3FE-4868520CD91E", "fa fa-clipboard" );
        }

        #endregion

        #region Administration Pages

        private void AddAdministrationPages()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Administration", "", "188D9FA7-1A6E-431F-9A79-79070285FF60", "fa fa-gear" );

            RockMigrationHelper.AddBlock( "188D9FA7-1A6E-431F-9A79-79070285FF60", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "DF70978D-0AB2-40F5-8935-AB3300541D4D" );
            RockMigrationHelper.AddBlockAttributeValue( "DF70978D-0AB2-40F5-8935-AB3300541D4D", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File
            RockMigrationHelper.AddBlockAttributeValue( "DF70978D-0AB2-40F5-8935-AB3300541D4D", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters
            RockMigrationHelper.AddBlockAttributeValue( "DF70978D-0AB2-40F5-8935-AB3300541D4D", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" ); // Template
            RockMigrationHelper.AddBlockAttributeValue( "DF70978D-0AB2-40F5-8935-AB3300541D4D", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels
            RockMigrationHelper.AddBlockAttributeValue( "DF70978D-0AB2-40F5-8935-AB3300541D4D", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString
            RockMigrationHelper.AddBlockAttributeValue( "DF70978D-0AB2-40F5-8935-AB3300541D4D", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue( "DF70978D-0AB2-40F5-8935-AB3300541D4D", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block
            RockMigrationHelper.AddBlockAttributeValue( "DF70978D-0AB2-40F5-8935-AB3300541D4D", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" ); // Include Page List

            AddServiceAreasPage();
            AddNotificationsPage();
            AddGlobalSettingsPage();
            AddDefinedTypesPage();
            AddAdvancedSettingsPage();
        }

        private void AddServiceAreasPage()
        {
            RockMigrationHelper.AddPage( "188D9FA7-1A6E-431F-9A79-79070285FF60", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Service Areas", "", "3F48E3B7-16E1-4721-B4B9-85041905E18E", "fa fa-map-signs" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Service Area List", "Allows for the managing of attribues.", "~/Plugins/org_willowcreek/CareCenter/ServiceAreaList.ascx", "org_willowcreek > CareCenter", "14626DC3-354B-4AD4-A1DB-8C97DF8A95E5" );
            RockMigrationHelper.AddBlockTypeAttribute( "14626DC3-354B-4AD4-A1DB-8C97DF8A95E5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "90CF3091-7CD9-4678-9966-60E6927786E5" );

            RockMigrationHelper.AddBlock( "3F48E3B7-16E1-4721-B4B9-85041905E18E", "", "14626DC3-354B-4AD4-A1DB-8C97DF8A95E5", "Service Area List", "Main", "", "", 0, "190260CD-C475-4E72-A53B-A10C17CF97FB" );
            RockMigrationHelper.AddBlockAttributeValue( "190260CD-C475-4E72-A53B-A10C17CF97FB", "90CF3091-7CD9-4678-9966-60E6927786E5", @"3282367f-f954-443c-bc93-a89a549d751f" ); // Detail Page

            AddServiceAreasDetailsPage();
        }

        private void AddServiceAreasDetailsPage()
        {
            RockMigrationHelper.AddPage( "3F48E3B7-16E1-4721-B4B9-85041905E18E", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Service Area Details", "", "3282367F-F954-443C-BC93-A89A549D751F", "fa-map-signs" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Service Area Detail", "Displays the details of the given financial account.", "~/Plugins/org_willowcreek/CareCenter/ServiceAreaDetail.ascx", "org_willowcreek > CareCenter", "30DA9A3B-DE3F-4354-91A2-F9C24D9F00B3" );
            RockMigrationHelper.AddBlockTypeAttribute( "30DA9A3B-DE3F-4354-91A2-F9C24D9F00B3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Appointment Time Slots Page", "TimeSlotPage", "", "The page used to manage the time slots that are available for appointments", 0, @"", "2AD037A7-A44E-4A23-AC47-CDF707797069" );

            RockMigrationHelper.AddBlock( "3282367F-F954-443C-BC93-A89A549D751F", "", "30DA9A3B-DE3F-4354-91A2-F9C24D9F00B3", "Service Area Detail", "Main", "", "", 0, "BB7A6FCD-A7E0-41E1-8485-2BF67466E58C" );
            RockMigrationHelper.AddBlockAttributeValue( "BB7A6FCD-A7E0-41E1-8485-2BF67466E58C", "2AD037A7-A44E-4A23-AC47-CDF707797069", @"5c6831a6-5ee9-491f-bc42-1b9995a2035e" ); // Appointment Time Slots Page

            RockMigrationHelper.UpdateBlockType( "Service Area Ban List", "User control for managing the Service Area Ban for the Care Center.", "~/Plugins/org_willowcreek/CareCenter/ServiceAreaBanList.ascx", "org_willowcreek > CareCenter", "74EECA83-1203-4C1E-8EE9-FC3F72836C86" );

            RockMigrationHelper.AddBlock( "3282367F-F954-443C-BC93-A89A549D751F", "", "74EECA83-1203-4C1E-8EE9-FC3F72836C86", "Service Area Ban List", "Main", "", "", 1, "D53930BB-0B17-44A8-85BC-D2878AA78C0A" );

            AddTimeSlotListPage();
        }

        private void AddTimeSlotListPage()
        {
            RockMigrationHelper.AddPage( "3282367F-F954-443C-BC93-A89A549D751F", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Time Slots", "", "5C6831A6-5EE9-491F-BC42-1B9995A2035E", "" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Appointment Time Slot List", "User control for managing the Service Area Time Slots for the Care Center.", "~/Plugins/org_willowcreek/CareCenter/TimeSlotList.ascx", "org_willowcreek > CareCenter", "6BB72201-4A07-480A-8C57-E5D3EED8D678" );
            RockMigrationHelper.AddBlockTypeAttribute( "6BB72201-4A07-480A-8C57-E5D3EED8D678", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "A4251190-5070-4220-85FF-423E8D5B2DC7" );

            RockMigrationHelper.AddBlock( "5C6831A6-5EE9-491F-BC42-1B9995A2035E", "", "6BB72201-4A07-480A-8C57-E5D3EED8D678", "Appointment Time Slot List", "Main", "", "", 0, "A5226EBB-4DC8-42D9-8C95-1268F2989561" );
            RockMigrationHelper.AddBlockAttributeValue( "A5226EBB-4DC8-42D9-8C95-1268F2989561", "A4251190-5070-4220-85FF-423E8D5B2DC7", @"a2c830d9-deed-40c1-93b5-6f25b0345c2b" ); // Detail Page

            AddTimeSlotDetailPage();
        }

        private void AddTimeSlotDetailPage()
        {
            RockMigrationHelper.AddPage( "5C6831A6-5EE9-491F-BC42-1B9995A2035E", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Time Slot Detail", "", "A2C830D9-DEED-40C1-93B5-6F25B0345C2B", "" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Appointment Time Slot Detail", "User control for managing the Service Area Time Slot for the Care Center.", "~/Plugins/org_willowcreek/CareCenter/TimeSlotDetail.ascx", "org_willowcreek > CareCenter", "023510C9-8524-4CCB-AB4E-F19626A29D27" );

            RockMigrationHelper.AddBlock( "A2C830D9-DEED-40C1-93B5-6F25B0345C2B", "", "023510C9-8524-4CCB-AB4E-F19626A29D27", "Appointment Time Slot Detail", "Main", "", "", 0, "1A0BA2FB-A211-4ED9-9D78-3198FB238ADD" );
        }

        private void AddNotificationsPage()
        {
            RockMigrationHelper.AddPage( "188D9FA7-1A6E-431F-9A79-79070285FF60", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Appointment Notifications", "", "1E6B110C-71E8-40BE-910E-72E93E7761CF", "fa fa-comment" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Appointment Notification List", "User control for managing the Appointment Notifications that are available for the Care Center.", "~/Plugins/org_willowcreek/CareCenter/AppointmentNotificationList.ascx", "org_willowcreek > CareCenter", "EBB7DE43-104C-4F6E-B2C3-8ED864AA0894" );
            RockMigrationHelper.AddBlockTypeAttribute( "EBB7DE43-104C-4F6E-B2C3-8ED864AA0894", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "F8152EEA-46FE-4FED-A783-D6CC3FA0EF72" );

            RockMigrationHelper.AddBlock( "1E6B110C-71E8-40BE-910E-72E93E7761CF", "", "EBB7DE43-104C-4F6E-B2C3-8ED864AA0894", "Appointment Notification List", "Main", "", "", 0, "248343DA-E370-4DF2-806D-47A78F1F8127" );
            RockMigrationHelper.AddBlockAttributeValue( "248343DA-E370-4DF2-806D-47A78F1F8127", "F8152EEA-46FE-4FED-A783-D6CC3FA0EF72", @"938d3ee0-18d0-4eca-a63f-a683acbefbde" );

            AddNotificationsDetailPage();
        }

        private void AddNotificationsDetailPage()
        {
            RockMigrationHelper.AddPage( "1E6B110C-71E8-40BE-910E-72E93E7761CF", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Notification Details", "", "938D3EE0-18D0-4ECA-A63F-A683ACBEFBDE", "fa fa-comment" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Appointment Notification Detail", "Displays the details of the given appointment notification.", "~/Plugins/org_willowcreek/CareCenter/AppointmentNotificationDetail.ascx", "org_willowcreek > CareCenter", "4EEDAF8A-9D69-4CC6-B33A-D7853D51F77A" );
            RockMigrationHelper.AddBlock( "938D3EE0-18D0-4ECA-A63F-A683ACBEFBDE", "", "4EEDAF8A-9D69-4CC6-B33A-D7853D51F77A", "Appointment Notification Detail", "Main", "", "", 0, "A91982CB-1C91-43E1-BE68-AA5DBD070CC5" );
        }

        private void AddGlobalSettingsPage()
        {
            RockMigrationHelper.AddPage( "188D9FA7-1A6E-431F-9A79-79070285FF60", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Global Settings", "", "C476D769-CCF9-4E94-A4D4-795C5EBCA165", "fa fa-list" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "", "A comma separated list of category guids to limit the display of attributes to.", 4, @"", "0C2BCD33-05CC-4B03-9F57-C686B8911E64" );

            RockMigrationHelper.AddBlock( "C476D769-CCF9-4E94-A4D4-795C5EBCA165", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Attributes", "Main", "", "", 0, "92EF8AD8-0524-446A-AB0B-72777C71C865" );
            RockMigrationHelper.AddBlockAttributeValue( "92EF8AD8-0524-446A-AB0B-72777C71C865", "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106", @"" ); // Entity Qualifier Column
            RockMigrationHelper.AddBlockAttributeValue( "92EF8AD8-0524-446A-AB0B-72777C71C865", "FCE1E87D-F816-4AD5-AE60-1E71942C547C", @"" ); // Entity Qualifier Value
            RockMigrationHelper.AddBlockAttributeValue( "92EF8AD8-0524-446A-AB0B-72777C71C865", "CBB56D68-3727-42B9-BF13-0B2B593FB328", @"0" ); // Entity Id
            RockMigrationHelper.AddBlockAttributeValue( "92EF8AD8-0524-446A-AB0B-72777C71C865", "018C0016-C253-44E4-84DB-D166084C5CAD", @"False" ); // Allow Setting of Values
            RockMigrationHelper.AddBlockAttributeValue( "92EF8AD8-0524-446A-AB0B-72777C71C865", "D4132497-18BE-4D1F-8913-468E33DE63C4", @"True" ); // Configure Type
            RockMigrationHelper.AddBlockAttributeValue( "92EF8AD8-0524-446A-AB0B-72777C71C865", "920FE120-AD75-4D5C-BFE0-FA5745B1118B", @"False" ); // Enable Show In Grid
            RockMigrationHelper.AddBlockAttributeValue( "92EF8AD8-0524-446A-AB0B-72777C71C865", "B45C5FA0-B969-4279-8A9F-710BBAB74D1F", @"False" ); // Enable Ordering
            RockMigrationHelper.AddBlockAttributeValue( "92EF8AD8-0524-446A-AB0B-72777C71C865", "0C2BCD33-05CC-4B03-9F57-C686B8911E64", @"0785DD67-C5A2-7C8A-42A6-97F8A470C02C" ); // Category Filter
        }

        private void AddDefinedTypesPage()
        {
            RockMigrationHelper.AddPage( "188D9FA7-1A6E-431F-9A79-79070285FF60", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Defined Types", "", "BA11FEAF-B2F7-4EBA-9A03-062DCBF78A2C", "fa fa-book" ); // Site:Care Center

            RockMigrationHelper.UpdateBlockType( "Defined Type List", "Lists all the defined types and allows for managing them and their values.", "~/Blocks/Core/DefinedTypeList.ascx", "Core", "5470C9C4-09C1-439F-AA56-3524047497EE" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "5470C9C4-09C1-439F-AA56-3524047497EE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "E75A3154-22F5-4E35-BF86-B57EFA460BD6" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "5470C9C4-09C1-439F-AA56-3524047497EE", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Categories", "Categories", "", "If block should only display Defined Types from specific categories, select the categories here.", 1, @"", "48F799FB-61F0-429E-A6DD-BB14393F18D3" );

            RockMigrationHelper.AddBlock( "BA11FEAF-B2F7-4EBA-9A03-062DCBF78A2C", "", "5470C9C4-09C1-439F-AA56-3524047497EE", "Defined Type List", "Main", "", "", 0, "8094E628-A81A-4326-B2EB-31133252FE69" );
            RockMigrationHelper.AddBlockAttributeValue( "8094E628-A81A-4326-B2EB-31133252FE69", "E75A3154-22F5-4E35-BF86-B57EFA460BD6", @"4d560671-b852-4578-9949-6118248e2501" ); // Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "8094E628-A81A-4326-B2EB-31133252FE69", "48F799FB-61F0-429E-A6DD-BB14393F18D3", @"178805fc-b70c-56a7-43df-22d17552993b,0a8ad02f-0f92-47e9-ad33-110aeaeb27c0" ); // Categories

            AddDefinedTypesDetailPage();
        }

        private void AddDefinedTypesDetailPage()
        {
            // Page: Defined Type Detail
            RockMigrationHelper.AddPage( "BA11FEAF-B2F7-4EBA-9A03-062DCBF78A2C", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Defined Type Detail", "", "4D560671-B852-4578-9949-6118248E2501", "fa fa-file-text" ); // Site:Care Center

            RockMigrationHelper.AddBlock( "4D560671-B852-4578-9949-6118248E2501", "", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "Main", "", "", 0, "0004DE77-E90C-446E-BDC1-1178367942C7" );

            RockMigrationHelper.AddBlock( "4D560671-B852-4578-9949-6118248E2501", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "Main", "", "", 1, "5E4EBC81-3578-4F74-9510-B3CF1FCF38E2" );
        }

        private void AddAdvancedSettingsPage()
        {
            RockMigrationHelper.AddPage( "188D9FA7-1A6E-431F-9A79-79070285FF60", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Advanced Settings", "", "524ECA00-88D1-42E1-9950-B1DB81B3E2A9", "fa fa-cogs" ); // Site:Care Center

            RockMigrationHelper.AddBlock( "524ECA00-88D1-42E1-9950-B1DB81B3E2A9", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Advanced Settings", "Main", "", "", 0, "1C00FD80-2740-4F4D-90E0-CD50A61FF297" );
            RockMigrationHelper.UpdateHtmlContentBlock( "1C00FD80-2740-4F4D-90E0-CD50A61FF297", @"
<div class='row'>
    <div class='col-md-4 col-sm-6 actions'>
        <a class='clear-coat-dates btn btn-primary pull-left margin-r-md' href='~/page/532'><i class='fa fa-eraser'></i> Clear Coat Dates</a>
        This will reset every person's coat date so that they are eligible again for a coat.
    </div>
</div>

<script>
    $('a.clear-coat-dates').click(function(e){
        e.preventDefault();
        Rock.dialogs.confirm( 'Are you sure you want to clear all of the existing coat dates? This will make every person eligible again to receive a coat.', 
            function(result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
</script>
", "1AD1A09F-08C5-4F65-A53A-9B98A213CF5A" );

            AddCoatResetPage();
        }

        private void AddCoatResetPage()
        {
            RockMigrationHelper.AddPage( "524ECA00-88D1-42E1-9950-B1DB81B3E2A9", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Coat Dates", "", "AA1F661C-D561-4451-BDE2-653B82EA1982", "" ); // Site:Care Center
            RockMigrationHelper.AddBlock( "AA1F661C-D561-4451-BDE2-653B82EA1982", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Clear Coat Dates", "Main", "", "", 0, "964812C5-82B5-449F-BFD8-CC7044433C14" );
            RockMigrationHelper.UpdateHtmlContentBlock( "964812C5-82B5-449F-BFD8-CC7044433C14", @"
{% sql statement:'command' %}
    DECLARE @AttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '20BCCDF0-CECF-FCAC-4172-5DF0D3A93D30' )
    DELETE [AttributeValue] 
    WHERE [AttributeId] = @AttributeId    
{% endsql %}

<div class='alert alert-info'>
    {{ results }} coat {{ 'record' | PluralizeForQuantity:results }} were cleared.
</div>
", "72EC0A05-5996-467D-A757-15FF4FA955C2" );

            Sql( @"
    DECLARE @BlockId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '1C00FD80-2740-4F4D-90E0-CD50A61FF297' )
    DECLARE @PageId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'AA1F661C-D561-4451-BDE2-653B82EA1982' )
    IF @BlockId IS NOT NULL AND @PageId IS NOT NULL 
    BEGIN
        UPDATE [HtmlContent] 
        SET [Content] = REPLACE( [Content], 'page/532', 'page/' + CAST(@PageId AS varchar) )
        WHERE [BlockId] = @BlockId
    END
" );
        }

        #endregion

        #region Support Pages

        private void AddSupportPage()
        {
            RockMigrationHelper.AddPage( "FD92FE87-D749-40F6-8161-77CFD8431AFB", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Support Pages", "", "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "" );
            Sql( @"
    UPDATE [Page]
    SET 
        [DisplayInNavWhen] = 2,
        [BreadCrumbDisplayName] = 0
    WHERE [Guid] = '612B0092-C7D2-4DCF-A9F2-CD8C629B14CA' 
" );

            AddLoginPage();
            AddForgotPasswordPage();
            AddChangePasswordPage();
            AddMyAccountPage();
            AddAdminIntakePage();
            AddPersonSearchPage();
            AddNewFamilyPage();
            AddAssessmentPage();
        }

        private void AddLoginPage()
        {
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Login", "", "3012B22F-912F-4C47-B8BC-219E1A0CE692", "" );
            RockMigrationHelper.AddPageRoute( "3012B22F-912F-4C47-B8BC-219E1A0CE692", "Login", "43AC6B7D-F00D-4A6C-B662-F906182D42C3" );// for Page:Login

            RockMigrationHelper.AddBlock( "3012B22F-912F-4C47-B8BC-219E1A0CE692", "", "7B83D513-1178-429E-93FF-E76430E038E4", "Login", "Main", "", "", 0, "8597A252-D1D1-44FB-B941-952112AB3F4F" );
            RockMigrationHelper.AddBlockAttributeValue( "8597A252-D1D1-44FB-B941-952112AB3F4F", "7D76CF1B-DFC7-47C0-AB55-C5CBDFAC811D", @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411" );
            RockMigrationHelper.AddBlockAttributeValue( "8597A252-D1D1-44FB-B941-952112AB3F4F", "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107", @"Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.
" );
            RockMigrationHelper.AddBlockAttributeValue( "8597A252-D1D1-44FB-B941-952112AB3F4F", "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9", @"Sorry, your account has been locked.  Please contact our office at {{ 'Global' | Attribute:'OrganizationPhone' }} or email {{ 'Global' | Attribute:'OrganizationEmail' }} to resolve this.  Thank-you. 
" );
            RockMigrationHelper.AddBlockAttributeValue( "8597A252-D1D1-44FB-B941-952112AB3F4F", "7D47046D-5D66-45BB-ACFA-7460DE112FC2", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "8597A252-D1D1-44FB-B941-952112AB3F4F", "8A09E6E2-3A9C-4D70-B03D-43D8FCB77D78", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "8597A252-D1D1-44FB-B941-952112AB3F4F", "B2ABA418-32EF-4310-A1EA-3C76A2375979", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "8597A252-D1D1-44FB-B941-952112AB3F4F", "AC8D3DB7-56F2-4BA0-B92A-4C2DCCB79AFE", @"Register" );

        }

        private void AddForgotPasswordPage()
        {
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Forgot Password", "", "F6AB8B2A-5431-4CC7-A927-F4CA7F38317E", "" );
            RockMigrationHelper.AddPageRoute( "F6AB8B2A-5431-4CC7-A927-F4CA7F38317E", "ForgotUserName", "62029A2D-9671-4514-B20B-7EB8FD9F42F7" );// for Page:Forgot Password

            RockMigrationHelper.AddBlock( "F6AB8B2A-5431-4CC7-A927-F4CA7F38317E", "", "02B3D7D1-23CE-4154-B602-F4A15B321757", "Forgot UserName", "Main", "", "", 0, "6FCBB239-EDEC-464A-AD53-8F85D192939B" );
            RockMigrationHelper.AddBlockAttributeValue( "6FCBB239-EDEC-464A-AD53-8F85D192939B", "488E438F-3BA3-4D3B-A1B0-D11D85752E06", @"Your user name has been sent with instructions on how to change your password if needed." );
            RockMigrationHelper.AddBlockAttributeValue( "6FCBB239-EDEC-464A-AD53-8F85D192939B", "6EFAF3CD-327A-4472-AA20-09AF1EF8BC78", @"<div class='alert alert-info'>Enter your email address below and we'll send your account information to you right away.</div>" );
            RockMigrationHelper.AddBlockAttributeValue( "6FCBB239-EDEC-464A-AD53-8F85D192939B", "87E7485A-FF22-48E7-BB4A-58E66B305D62", @"Sorry, we could not find an account for the email address you entered." );
        }

        private void AddChangePasswordPage()
        {
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Change Password", "", "D4E16ECC-FC79-412B-A35C-F9147E9341A2", "" );
            RockMigrationHelper.AddBlock( "D4E16ECC-FC79-412B-A35C-F9147E9341A2", "", "3C12DE99-2D1B-40F2-A9B8-6FE7C2524B37", "Change Password", "Main", "", "", 0, "901A83C2-C81A-4FF4-88AF-8E029C42AAF7" );
        }

        private void AddMyAccountPage()
        {
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "CF031F3F-3375-4A29-93FC-A97B15864208", "My Account", "", "137DCC41-D3CE-4A17-B721-54D2C60F5493", "" );
        }

        private void AddAdminIntakePage()
        {
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Intake", "", "54691BA5-B00C-490A-B7C1-4C86EB2BA5A9", "fa fa-clipboard" );
            RockMigrationHelper.AddPageRoute( "54691BA5-B00C-490A-B7C1-4C86EB2BA5A9", "Intake/{PersonId}", "93C29356-F283-41AB-91B1-43DADCD464C3" );

            RockMigrationHelper.UpdateBlockType( "Intake Form", "Care Center block for new visitors.", "~/Plugins/org_willowcreek/CareCenter/Intake.ascx", "org_willowcreek > CareCenter", "82FE4BB4-F028-45A8-AA21-05C6A9BAB443" );
            RockMigrationHelper.AddBlockTypeAttribute( "82FE4BB4-F028-45A8-AA21-05C6A9BAB443", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Page", "PersonPage", "", "The page to return to after intake is cancelled.", 0, @"", "25490D41-3063-43BF-AE35-CE3A39CEA8D8" );
            RockMigrationHelper.AddBlockTypeAttribute( "82FE4BB4-F028-45A8-AA21-05C6A9BAB443", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Search Page", "SearchPage", "", "The page to return to after intake is completed.", 1, @"", "D9D3F85F-1003-4CF7-AD61-FAB539BDC3BC" );
            RockMigrationHelper.AddBlockTypeAttribute( "82FE4BB4-F028-45A8-AA21-05C6A9BAB443", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Appointment Page", "AppointmentPage", "", "The page to navigate to if an appointment is being scheduled at end of Intake.", 2, @"", "3420C926-50E5-4280-91C0-E4CDE87C0FCB" );
            RockMigrationHelper.AddBlockTypeAttribute( "82FE4BB4-F028-45A8-AA21-05C6A9BAB443", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Family Members", "ShowFamilyMembers", "", "Should the person's family member names be displayed?", 3, @"False", "AB99B77F-0D0D-45D2-8AE6-640770D263EF" );
            RockMigrationHelper.AddBlockTypeAttribute( "82FE4BB4-F028-45A8-AA21-05C6A9BAB443", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Courtesy Visit", "AllowCourtesyVisit", "", "Should the courtesy food option be available?", 4, @"False", "5E89CD57-DCF0-4A1A-8CE0-66123F37D2C9" );

            RockMigrationHelper.AddBlock( "54691BA5-B00C-490A-B7C1-4C86EB2BA5A9", "", "82FE4BB4-F028-45A8-AA21-05C6A9BAB443", "Intake Form", "Main", "", "", 0, "53D813B7-C658-47EC-9268-2341B047D2EF" );
            RockMigrationHelper.AddBlockAttributeValue( "53D813B7-C658-47EC-9268-2341B047D2EF", "25490D41-3063-43BF-AE35-CE3A39CEA8D8", @"b3322488-717f-459f-b63f-813c68a78db9,f63deb87-96d5-4480-9688-5a4d43904301" ); // Person Page
            RockMigrationHelper.AddBlockAttributeValue( "53D813B7-C658-47EC-9268-2341B047D2EF", "D9D3F85F-1003-4CF7-AD61-FAB539BDC3BC", @"a5b11b6a-6f8b-48de-8bbb-65b5526f7f75" ); // Search Page
            RockMigrationHelper.AddBlockAttributeValue( "53D813B7-C658-47EC-9268-2341B047D2EF", "3420C926-50E5-4280-91C0-E4CDE87C0FCB", @"1b0052da-6138-4e53-947d-8bb5f6367aa6" ); // Appointment Page
            RockMigrationHelper.AddBlockAttributeValue( "53D813B7-C658-47EC-9268-2341B047D2EF", "5E89CD57-DCF0-4A1A-8CE0-66123F37D2C9", @"False" ); // Allow Courtesy Visit
            RockMigrationHelper.AddBlockAttributeValue( "53D813B7-C658-47EC-9268-2341B047D2EF", "AB99B77F-0D0D-45D2-8AE6-640770D263EF", @"False" ); // Show Family Members
        }

        private void AddPersonSearchPage()
        {
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Person Search", "", "C12A45EA-8474-4D00-A998-28241235E675", "" ); // Site:Care Center Internal
            RockMigrationHelper.AddPageRoute( "C12A45EA-8474-4D00-A998-28241235E675", "Person/Search/{SearchType}", "3CED9D24-8E24-4002-977B-E085E0549B6D" );// for Page:Person Search

            RockMigrationHelper.AddBlock( "C12A45EA-8474-4D00-A998-28241235E675", "", "764D3E67-2D01-437A-9F45-9F8C97878434", "Person Search", "Main", "", "", 0, "C0071BE9-B419-4CDB-A8C8-A79EB89AF552" );
            RockMigrationHelper.AddBlockAttributeValue( "C0071BE9-B419-4CDB-A8C8-A79EB89AF552", "F6E44A84-AFDD-4F66-9ED3-64ECF45BC7DA", @"b3322488-717f-459f-b63f-813c68a78db9,f63deb87-96d5-4480-9688-5a4d43904301" ); // Person Detail Page

            AddPersonPages();
        }

        private void AddPersonPages()
        {
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Person Pages", "", "1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE", "" ); // Site:Care Center Internal

            Sql( @"
    UPDATE [Page]
    SET 
        [DisplayInNavWhen] = 2,
        [BreadCrumbDisplayName] = 0
    WHERE [Guid] = '1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE' 
" );

            AddPersonProfilePage();
            AddAttributesPage();
            AddBenevolencePage();
            AddCareTeamTabPage();
            AddEmploymentTabPage();
            AddFinancialTabPage();
            AddHistoryPage();
        }

        private void AddPersonProfilePage()
        {
            RockMigrationHelper.AddPage( "1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "Person Profile", "", "B3322488-717F-459F-B63F-813C68A78DB9", "" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "B3322488-717F-459F-B63F-813C68A78DB9", "Person/{PersonId}" );
            RockMigrationHelper.UpdatePageContext( "B3322488-717F-459F-B63F-813C68A78DB9", "Rock.Model.Person", "PersonId", "4BF6F1AA-F770-4493-8F1A-0FD4D6238FC2" );

            RockMigrationHelper.UpdateBlockType( "Notes", "Context aware block for adding notes to an entity.", "~/Blocks/Core/Notes.ascx", "Core", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3" );

            RockMigrationHelper.UpdateBlockType( "My Visits", "Care Center block for viewing list of visits for a specific persons family.", "~/Plugins/org_willowcreek/CareCenter/MyVisits.ascx", "org_willowcreek > CareCenter", "02F0B53E-205E-47E6-9264-3F2DE64437B3" );
            RockMigrationHelper.AddBlockTypeAttribute( "02F0B53E-205E-47E6-9264-3F2DE64437B3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Items", "MaxItems", "", "The maximum number of visits to show in the list", 0, @"10", "EEC00729-28D8-427D-933D-1003B7AA4B2B" );
            RockMigrationHelper.AddBlockTypeAttribute( "02F0B53E-205E-47E6-9264-3F2DE64437B3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Visit List Page", "VisitListPage", "", "Page for displaying all of the visits", 1, @"", "350A95EE-76BE-4A0A-9A74-3615B1087D82" );
            RockMigrationHelper.AddBlockTypeAttribute( "02F0B53E-205E-47E6-9264-3F2DE64437B3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Visit Detail Page", "VisitDetailPage", "", "Page for displaying details of a specific visit", 2, @"", "22797180-6A2E-4F3F-8E2B-D883D07B2F7E" );

            RockMigrationHelper.UpdateBlockType( "My Appointments", "Care Center block for viewing list of appointments for a specific persons family.", "~/Plugins/org_willowcreek/CareCenter/MyAppointments.ascx", "org_willowcreek > CareCenter", "B3918663-BBD1-4EE6-802B-EB9343C15CBE" );
            RockMigrationHelper.AddBlockTypeAttribute( "B3918663-BBD1-4EE6-802B-EB9343C15CBE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Items", "MaxItems", "", "The maximum number of appointments to show in the list", 0, @"10", "CB32D190-59B8-42DF-9554-C12134BD84FC" );
            RockMigrationHelper.AddBlockTypeAttribute( "B3918663-BBD1-4EE6-802B-EB9343C15CBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Appointment List Page", "AppointmentListPage", "", "Page for displaying all of the appointments", 1, @"", "5DF67411-7DA8-4CE0-8DD5-85F4CF708942" );
            RockMigrationHelper.AddBlockTypeAttribute( "B3918663-BBD1-4EE6-802B-EB9343C15CBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Appointment Detail Page", "AppointmentDetailPage", "", "Page for displaying details of a specific appointment", 2, @"", "827C299E-6678-4F43-B2F9-784B8B0BEBBF" );

            RockMigrationHelper.UpdateBlockType( "My Assessments", "Care Center block for viewing list of assessments for a specific persons family.", "~/Plugins/org_willowcreek/CareCenter/MyAssessments.ascx", "org_willowcreek > CareCenter", "33C93BE9-368C-41C9-893D-D6EF1973F440" );
            RockMigrationHelper.AddBlockTypeAttribute( "33C93BE9-368C-41C9-893D-D6EF1973F440", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Assessment List Page", "AssessmentListPage", "", "Page for displaying all of the assessments", 1, @"", "ABFDCDCD-6D3D-4B64-A061-6E1823859E7A" );
            RockMigrationHelper.AddBlockTypeAttribute( "33C93BE9-368C-41C9-893D-D6EF1973F440", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Assessment Detail Page", "AssessmentDetailPage", "", "Page for displaying details of a specific assessment", 2, @"", "2CE6B7CB-FBA7-4650-AE00-907B9D0AC256" );

            RockMigrationHelper.AddBlock( "B3322488-717F-459F-B63F-813C68A78DB9", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "TimeLine", "SectionA1", "", "", 0, "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F" );
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"72657ed8-d16e-492e-ac12-144c5e7567e7" ); // Entity Type
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" ); // Show Private Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" ); // Show Security Button
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "20243A98-4802-48E2-AF61-83956056AC65", @"True" ); // Show Alert Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Timeline" ); // Heading
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-quote-left" ); // Heading Icon CSS Class
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" ); // Note Term
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" ); // Display Type
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" ); // Use Person Icon
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" ); // Allow Anonymous
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" ); // Add Always Visible
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" ); // Display Order
            RockMigrationHelper.AddBlockAttributeValue( "E3BE2FDD-6A32-4D1E-81C6-EF9B0F59682F", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" ); // Allow Backdated Notes

            RockMigrationHelper.AddBlock( "B3322488-717F-459F-B63F-813C68A78DB9", "", "02F0B53E-205E-47E6-9264-3F2DE64437B3", "My Visits", "SectionA2", "", "", 0, "F38D5BDC-794E-4DED-920E-2BB61375BB65" );
            RockMigrationHelper.AddBlockAttributeValue( "F38D5BDC-794E-4DED-920E-2BB61375BB65", "22797180-6A2E-4F3F-8E2B-D883D07B2F7E", @"2a06d3e9-ef49-4d0f-b3de-d3845f1abb07" ); // Visit Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "F38D5BDC-794E-4DED-920E-2BB61375BB65", "EEC00729-28D8-427D-933D-1003B7AA4B2B", @"10" ); // Max Items
            RockMigrationHelper.AddBlockAttributeValue( "F38D5BDC-794E-4DED-920E-2BB61375BB65", "350A95EE-76BE-4A0A-9A74-3615B1087D82", @"1033a239-5626-4dc6-997f-180c951c6f3d" ); // Visit List Page

            RockMigrationHelper.AddBlock( "B3322488-717F-459F-B63F-813C68A78DB9", "", "B3918663-BBD1-4EE6-802B-EB9343C15CBE", "My Appointments", "SectionA2", "", "", 1, "1A4A2C03-9F8E-4860-BD60-68E36E539D20" );
            RockMigrationHelper.AddBlockAttributeValue( "1A4A2C03-9F8E-4860-BD60-68E36E539D20", "827C299E-6678-4F43-B2F9-784B8B0BEBBF", @"fbc943c6-52f4-4b47-a881-add53cf7bb1b" ); // Appointment Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "1A4A2C03-9F8E-4860-BD60-68E36E539D20", "CB32D190-59B8-42DF-9554-C12134BD84FC", @"10" ); // Max Items
            RockMigrationHelper.AddBlockAttributeValue( "1A4A2C03-9F8E-4860-BD60-68E36E539D20", "5DF67411-7DA8-4CE0-8DD5-85F4CF708942", @"aed66d1b-97b8-491a-b9bd-a7cfd4cc182f" ); // Appointment List Page

            RockMigrationHelper.AddBlock( "B3322488-717F-459F-B63F-813C68A78DB9", "", "33C93BE9-368C-41C9-893D-D6EF1973F440", "My Assessments", "SectionA2", "", "", 2, "16440C81-D197-4B36-B0E4-ACA4A9F32F7B" );
            RockMigrationHelper.AddBlockAttributeValue( "16440C81-D197-4B36-B0E4-ACA4A9F32F7B", "ABFDCDCD-6D3D-4B64-A061-6E1823859E7A", @"799bdfe5-ef56-48ba-80b7-843ea2ad5e1b,14637b82-390b-4122-8f8d-cbd2da53d7b1" ); // Assessment List Page
            RockMigrationHelper.AddBlockAttributeValue( "16440C81-D197-4B36-B0E4-ACA4A9F32F7B", "2CE6B7CB-FBA7-4650-AE00-907B9D0AC256", @"2eb1e4f1-b4de-426f-9372-3dfb3d6cd22f" ); // Assessment Detail Page

            AddEditPersonPage();
            AddEditFamilyPage();
            AddLocationSettingsPage();
        }

        private void AddEditPersonPage()
        {
            RockMigrationHelper.AddPage( "B3322488-717F-459F-B63F-813C68A78DB9", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Edit Person", "", "36696958-D5AF-4498-B834-35FCE8D88099", "" ); // Site:Care Center Internal
            RockMigrationHelper.AddPageRoute( "36696958-D5AF-4498-B834-35FCE8D88099", "Person/{PersonId}/Edit", "93D34399-D538-4243-A2CF-0BE9ECF43352" );// for Page:Person Edit
            RockMigrationHelper.UpdatePageContext( "36696958-D5AF-4498-B834-35FCE8D88099", "Rock.Model.Person", "PersonId", "64A6CD79-810B-4120-A735-8AB622B1C7AD" );

            RockMigrationHelper.AddBlock( "36696958-D5AF-4498-B834-35FCE8D88099", "", "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "Edit Person", "Main", "", "", 0, "C90CCF68-FD43-4707-A30E-681A22A1526C" );
        }

        private void AddEditFamilyPage()
        {
            RockMigrationHelper.AddPage( "B3322488-717F-459F-B63F-813C68A78DB9", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Edit Family", "", "6EC89E41-8C1D-4435-B5CD-0F8D876379C6", "" ); // Site:Care Center Internal
            RockMigrationHelper.AddPageRoute( "6EC89E41-8C1D-4435-B5CD-0F8D876379C6", "EditFamily/{PersonId}/{GroupId}", "8181DDA4-E8A1-4D26-9AB1-E54AC4F34184" );// for Page:Edit Family
            RockMigrationHelper.UpdatePageContext( "6EC89E41-8C1D-4435-B5CD-0F8D876379C6", "Rock.Model.Person", "PersonId", "102823BD-FFCC-452B-A5B7-AB84CE121171" );

            RockMigrationHelper.AddBlock( "6EC89E41-8C1D-4435-B5CD-0F8D876379C6", "", "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "Edit Group", "Main", "", "", 0, "6A7036DB-4A64-44D7-B829-816E7B6F54C6" );
        }

        private void AddLocationSettingsPage()
        {
            RockMigrationHelper.AddPage( "B3322488-717F-459F-B63F-813C68A78DB9", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Location Settings", "", "47C5937B-2B96-4167-88A9-9BF08B2EC496", "" ); // Site:Care Center Internal
            RockMigrationHelper.AddBlock( "47C5937B-2B96-4167-88A9-9BF08B2EC496", "", "08189564-1245-48F8-86CC-560F4DD48733", "Location Detail", "Main", "", "", 0, "98EF1835-8FB6-400E-98DB-DDAAA240AA43" );
        }

        private void AddAttributesPage()
        {
            RockMigrationHelper.AddPage( "1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "Attributes", "", "F2535888-2C5F-4809-9A25-A56155480FA1", "" ); // Site:Care Center Internal
            RockMigrationHelper.AddPageRoute( "F2535888-2C5F-4809-9A25-A56155480FA1", "Person/{PersonId}/Attributes", "BEA2CDCC-FE49-4321-A604-DADCB0482992" );// for Page:Attributes
            RockMigrationHelper.UpdatePageContext( "F2535888-2C5F-4809-9A25-A56155480FA1", "Rock.Model.Person", "PersonId", "1933F012-C48F-4E24-9F03-CDE0E3F30B24" );

            RockMigrationHelper.AddBlock( "F2535888-2C5F-4809-9A25-A56155480FA1", "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "Attribute Values", "SectionB1", "", "", 0, "BB0DF014-C3A6-4AC8-822E-00DA78781161" );
            RockMigrationHelper.AddBlockAttributeValue( "BB0DF014-C3A6-4AC8-822E-00DA78781161", "B7EB7168-DEAD-4BD0-A854-B94BC5BDE06E", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "BB0DF014-C3A6-4AC8-822E-00DA78781161", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"a498fef0-cca3-dea4-413f-080ec00b1ac3" );

            RockMigrationHelper.AddBlock( "F2535888-2C5F-4809-9A25-A56155480FA1", "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "Attribute Values", "SectionB2", "", "", 0, "40CA2FC4-12C9-4783-898D-02228FDFA36C" );
            RockMigrationHelper.AddBlockAttributeValue( "40CA2FC4-12C9-4783-898D-02228FDFA36C", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"d61f30ec-35d9-fe90-4e84-26fbf3d72c8b" );
            RockMigrationHelper.AddBlockAttributeValue( "40CA2FC4-12C9-4783-898D-02228FDFA36C", "B7EB7168-DEAD-4BD0-A854-B94BC5BDE06E", @"" );
        }

        private void AddBenevolencePage()
        {
            RockMigrationHelper.AddPage( "1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "Benevolence", "", "09243995-A2DB-46D7-9D8B-569E638DC62C", "" ); // Site:Care Center Internal
            RockMigrationHelper.AddPageRoute( "09243995-A2DB-46D7-9D8B-569E638DC62C", "Person/{PersonId}/Benevolence", "22E587E8-33C1-4B41-9E59-BEEB1AF71E77" );// for Page:Benevolence
            RockMigrationHelper.UpdatePageContext( "09243995-A2DB-46D7-9D8B-569E638DC62C", "Rock.Model.Person", "PersonId", "2A790D19-90F4-4D4C-8038-0F05030F12CD" );

            RockMigrationHelper.AddBlock( "09243995-A2DB-46D7-9D8B-569E638DC62C", "", "3131C55A-8753-435F-85F3-DF777EFBD1C8", "Benevolence Request List", "SectionC1", "", "", 0, "1F61D6F6-F37B-4A86-8328-FFDDE4E33D41" );
            RockMigrationHelper.AddBlockAttributeValue( "1F61D6F6-F37B-4A86-8328-FFDDE4E33D41", "E2C90243-A79A-4DAD-9301-07F6DF095CDB", @"c536ce5a-0cf3-4449-9875-91f66efa760e" ); // Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "1F61D6F6-F37B-4A86-8328-FFDDE4E33D41", "1D3A87F6-1A27-407E-9218-F92B2E08AEB8", @"02fa0881-3552-42b8-a519-d021139b800f" ); // Case Worker Role

            AddBenevolenceDetailPage();
        }

        private void AddBenevolenceDetailPage()
        {
            RockMigrationHelper.AddPage( "09243995-A2DB-46D7-9D8B-569E638DC62C", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Benevolence Request", "", "C536CE5A-0CF3-4449-9875-91F66EFA760E", "" ); // Site:Care Center

            RockMigrationHelper.AddBlock( "C536CE5A-0CF3-4449-9875-91F66EFA760E", "", "34275D0E-BC7E-4A9C-913E-623D086159A1", "Benevolence Request Detail", "Main", "", "", 0, "B20A1442-38B7-485C-AB8E-8FD50E718B34" );
            RockMigrationHelper.AddBlockAttributeValue( "B20A1442-38B7-485C-AB8E-8FD50E718B34", "4D11BFF0-D253-49F9-8AD4-6662452F4E5E", @"5827ee27-a62d-4fa2-ab94-4509cf45848b" ); // Benevolence Request Statement Page
            RockMigrationHelper.AddBlockAttributeValue( "B20A1442-38B7-485C-AB8E-8FD50E718B34", "1453CA30-40F6-4625-A5FF-8269AD603C85", @"02fa0881-3552-42b8-a519-d021139b800f" ); // Case Worker Role

            AddBenevolenceResultPage();
        }

        private void AddBenevolenceResultPage()
        {
            RockMigrationHelper.AddPage( "C536CE5A-0CF3-4449-9875-91F66EFA760E", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Benevolence Result", "", "5827EE27-A62D-4FA2-AB94-4509CF45848B", "" ); // Site:Care Center

            RockMigrationHelper.AddBlock( "5827EE27-A62D-4FA2-AB94-4509CF45848B", "", "C2D8FCA3-BC8F-44FF-85AA-440BF41CEF5D", "Benevolence Request Statement Lava", "Main", "", "", 0, "73F2F39B-D1F5-42AD-A3A6-E288D97F8150" );
        }

        private void AddCareTeamTabPage()
        {
            RockMigrationHelper.AddPage( "1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "Care Team", "", "61325623-BAEF-4D7D-9611-5E7FCF94875B", "" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "61325623-BAEF-4D7D-9611-5E7FCF94875B", "Person/{PersonId}/CareCenter" );
            RockMigrationHelper.UpdatePageContext( "61325623-BAEF-4D7D-9611-5E7FCF94875B", "Rock.Model.Person", "PersonId", "D6587A1A-B304-4A8A-A6D9-9718690DA8A8" );

            RockMigrationHelper.AddBlock( "61325623-BAEF-4D7D-9611-5E7FCF94875B", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "SectionC1", "", "", 0, "6417708A-37FC-4D07-B623-5383DDCA6C08" );
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"72657ed8-d16e-492e-ac12-144c5e7567e7" ); // Entity Type
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" ); // Show Private Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" ); // Show Security Button
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "20243A98-4802-48E2-AF61-83956056AC65", @"True" ); // Show Alert Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Care Team Notes" ); // Heading
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-sticky-note-o" ); // Heading Icon CSS Class
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" ); // Note Term
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" ); // Display Type
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" ); // Use Person Icon
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" ); // Allow Anonymous
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" ); // Add Always Visible
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" ); // Display Order
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" ); // Allow Backdated Notes
            RockMigrationHelper.AddBlockAttributeValue( "6417708A-37FC-4D07-B623-5383DDCA6C08", "03A0974C-7BC6-4531-B31A-381D74F2CD76", @"6E79DDC0-9A20-48D8-B5DE-C3A9F5636D42" ); // Note Types
        }

        private void AddEmploymentTabPage()
        {
            RockMigrationHelper.AddPage( "1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "Employment", "", "6177EC90-31D0-4324-B2E1-3E15A35180FB", "" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "6177EC90-31D0-4324-B2E1-3E15A35180FB", "Person/{PersonId}/Employment" );
            RockMigrationHelper.UpdatePageContext( "6177EC90-31D0-4324-B2E1-3E15A35180FB", "Rock.Model.Person", "PersonId", "41F11B08-A333-4BFF-ACB9-1826AB916923" );

            RockMigrationHelper.AddBlock( "6177EC90-31D0-4324-B2E1-3E15A35180FB", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "SectionC1", "", "", 0, "6043BFBD-B36D-4064-826A-3F64558C519B" );
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"72657ed8-d16e-492e-ac12-144c5e7567e7" ); // Entity Type
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" ); // Show Private Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" ); // Show Security Button
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "20243A98-4802-48E2-AF61-83956056AC65", @"True" ); // Show Alert Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Employment Notes" ); // Heading
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-sticky-note-o" ); // Heading Icon CSS Class
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" ); // Note Term
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" ); // Display Type
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" ); // Use Person Icon
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" ); // Allow Anonymous
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" ); // Add Always Visible
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" ); // Display Order
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" ); // Allow Backdated Notes
            RockMigrationHelper.AddBlockAttributeValue( "6043BFBD-B36D-4064-826A-3F64558C519B", "03A0974C-7BC6-4531-B31A-381D74F2CD76", @"D9234C0B-34AF-486A-A516-2DB9732EC22A" ); // Note Types
        }

        private void AddFinancialTabPage()
        {
            // Page: Financial
            RockMigrationHelper.AddPage( "1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "Financial", "", "B97A7E8C-2816-4167-B3E6-B70FDF235226", "" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "B97A7E8C-2816-4167-B3E6-B70FDF235226", "Person/{PersonId}/Financial" );
            RockMigrationHelper.UpdatePageContext( "B97A7E8C-2816-4167-B3E6-B70FDF235226", "Rock.Model.Person", "PersonId", "6A1EF92F-1A65-4283-9991-998386DFBE12" );

            RockMigrationHelper.AddBlock( "B97A7E8C-2816-4167-B3E6-B70FDF235226", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "SectionC1", "", "", 0, "AA10BDBF-66A4-4993-BED2-81C774B26B0B" );
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"72657ed8-d16e-492e-ac12-144c5e7567e7" ); // Entity Type
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" ); // Show Private Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" ); // Show Security Button
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "20243A98-4802-48E2-AF61-83956056AC65", @"True" ); // Show Alert Checkbox
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Financial Notes" ); // Heading
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-sticky-note-o" ); // Heading Icon CSS Class
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" ); // Note Term
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" ); // Display Type
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" ); // Use Person Icon
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" ); // Allow Anonymous
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" ); // Add Always Visible
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" ); // Display Order
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" ); // Allow Backdated Notes
            RockMigrationHelper.AddBlockAttributeValue( "AA10BDBF-66A4-4993-BED2-81C774B26B0B", "03A0974C-7BC6-4531-B31A-381D74F2CD76", @"208AB786-1CE9-42CB-93A4-5BB73AF41FE4" ); // Note Types
        }

        private void AddHistoryPage()
        {
            RockMigrationHelper.AddPage( "1B60AB7C-B1AA-40F9-B1A1-F0063B2229EE", "FA2CCF6F-F563-4023-A600-916F4FB1E1F0", "History", "", "2B015283-A641-4FEA-9D10-85E5CCFDCE68", "" ); // Site:Care Center Internal
            RockMigrationHelper.AddPageRoute( "2B015283-A641-4FEA-9D10-85E5CCFDCE68", "Person/{PersonId}/History", "779F6D42-64A2-4FC1-BD5B-858604A2568B" );// for Page:History
            RockMigrationHelper.UpdatePageContext( "2B015283-A641-4FEA-9D10-85E5CCFDCE68", "Rock.Model.Person", "PersonId", "44B4F752-6075-4E79-8961-770665549FD9" );

            RockMigrationHelper.AddBlock( "2B015283-A641-4FEA-9D10-85E5CCFDCE68", "", "854C7AE2-6FA4-4D1A-BBB5-012484EA436E", "Person History", "SectionC1", "", "", 0, "F059016D-B08B-4C00-B8AC-5CB83061AEB1" );
        }

        private void AddNewFamilyPage()
        {
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "New Family", "", "FF558FF5-3BD9-4E13-AA50-7F8417A88916", "fa fa-users" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "FF558FF5-3BD9-4E13-AA50-7F8417A88916", "NewFamily", "45DE53DD-269B-46FF-BACC-DF4CE16F0885" );

            RockMigrationHelper.AddBlock( "FF558FF5-3BD9-4E13-AA50-7F8417A88916", "", "DE156975-597A-4C55-A649-FE46712F91C3", "Add Group", "Main", "", "", 0, "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2" );
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "687E6973-0259-4DB4-B4EB-73708D98EFCD", @"8c52e53c-2a66-435a-ae6e-5ee307d9a0dc" ); // Location Type
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "49E4751F-9BBB-42DD-97B0-FA7DD2F50ADA", @"True" ); // Gender
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "5340120D-B914-4689-B915-9C25865A3637", @"" ); // Attribute Categories
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "521E95E3-286C-4B6A-BB7D-A7DBD803C81D", @"False" ); // Grade
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "34B56BE9-FF33-4F64-A4B2-A098EC5250FB", @"f19fc180-fe8f-4b72-a59c-8013e3b0eb0d" ); // Child Marital Status
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "815D526D-671A-48B0-8988-9264D65BAB26", @"" ); // Adult Marital Status
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "135036D6-6F3C-45A0-8FCA-42EA953C0255", @"af7f9659-d047-4eb6-97d4-31034a5801bc" ); // Default Connection Status
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "F349D3AE-9025-4F62-BB59-ED2D5BF525CF", @"790e3215-3b10-442b-af69-616c0dcb998e" ); // Group Type
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "5E62C385-C24B-4B02-A49D-803733DF9E05", @"True" ); // Marital Status Confirmation
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "C48243DC-CD23-4B7F-9857-A64C4DBDBF94", @"False" ); // SMS
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "E9A63530-E369-44EA-9F75-25908FF94155", @"True" ); // Enable Common Last Name
            RockMigrationHelper.AddBlockAttributeValue( "0C0211CD-4263-4FA2-BCDE-20F4D3C442F2", "0B65A011-D696-4A1A-9CCD-C3C98E1C8C94", @"True" ); // Show Inactive Campuses
        }

        private void AddAssessmentPage()
        {
            // Page: Assessment
            RockMigrationHelper.AddPage( "612B0092-C7D2-4DCF-A9F2-CD8C629B14CA", "0D7BA09A-A266-47C3-A1F8-A2E08945C1DC", "Assessment", "", "D04B18F1-289B-454A-BBFE-F95B103FDFF9", "" ); // Site:Care Center
            RockMigrationHelper.AddPageRoute( "D04B18F1-289B-454A-BBFE-F95B103FDFF9", "Assessment/{PersonId}" );
            RockMigrationHelper.AddPageRoute( "D04B18F1-289B-454A-BBFE-F95B103FDFF9", "Assessment" );
            RockMigrationHelper.UpdatePageContext( "D04B18F1-289B-454A-BBFE-F95B103FDFF9", "Rock.Model.Person", "PersonId", "1712C1D1-7490-4BF7-9809-2653B19F3CBC" );

            RockMigrationHelper.UpdateBlockType( "Assesment", "Block for starting an assessment.", "~/Plugins/org_willowcreek/CareCenter/Assesment.ascx", "org_willowcreek > CareCenter", "DA92D882-190A-4935-B212-6B369B764425" );
            RockMigrationHelper.AddBlockTypeAttribute( "DA92D882-190A-4935-B212-6B369B764425", "3F1AE891-7DC8-46D2-865D-11543B34FB60", "Badges", "Badges", "", "The label badges to display in this block.", 0, @"", "1068B3D3-D55A-4ADD-903F-035F27B3F7FE" );
            RockMigrationHelper.AddBlockTypeAttribute( "DA92D882-190A-4935-B212-6B369B764425", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Country Code", "DisplayCountryCode", "", "When enabled prepends the country code to all phone numbers.", 1, @"False", "F7EA0170-277B-4D43-92D8-7CD5CE13E66D" );
            RockMigrationHelper.AddBlockTypeAttribute( "DA92D882-190A-4935-B212-6B369B764425", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Family Edit Page", "FamilyEditPage", "", "Page used to edit the members of the selected family.", 2, @"", "6142B9DA-B237-4E16-8485-7F2FAE1C70F6" );
            RockMigrationHelper.AddBlockTypeAttribute( "DA92D882-190A-4935-B212-6B369B764425", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Middle Name", "DisplayMiddleName", "", "Display the middle name of the person.", 2, @"False", "11053FC1-85E4-4F32-BC02-A54A7278DE9B" );
            RockMigrationHelper.AddBlockTypeAttribute( "DA92D882-190A-4935-B212-6B369B764425", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "The page to return to after intake is completed.", 4, @"", "6F136BE0-408E-4226-9CFD-320EE6842EAA" );

            RockMigrationHelper.AddBlock( "D04B18F1-289B-454A-BBFE-F95B103FDFF9", "", "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "Campus Context Setter", "Main", "<div class='pull-right'>", "</div>", 0, "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166" );
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "A1F4E771-8A8C-46E6-9DDD-1603590B67E8", @"Page" ); // Context Scope
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "C894049D-3CBF-493C-8979-F2CD95AE8B77", @"1" ); // Alignment
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "C59D2067-25DF-4326-9D31-57BF42CB3FAF", @"True" ); // Default To Current User's Campus
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "EF03EFD4-5060-4459-91E1-A08D292105A7", @"{{ CampusName }}" ); // Current Item Template
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "D699CC76-F6AE-4E55-9961-EDADA6AF2382", @"{{ CampusName }}" ); // Dropdown Item Template
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "B67E8E99-AA76-4A1F-9E3C-A26DF7F23096", @"Select Campus" ); // No Campus Text
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "8565C17E-04F8-4E2B-90AC-5CF80FC07A59", @"" ); // Clear Selection Text
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "E14249C3-6EC7-4CBE-9C1D-611B79852186", @"False" ); // Display Query Strings
            RockMigrationHelper.AddBlockAttributeValue( "F0FF7FF3-5EB3-4E68-B4AC-A1A1795D8166", "0587E3AE-F856-41A4-951D-8E3308F69AFC", @"False" ); // Include Inactive Campuses

            RockMigrationHelper.AddBlock( "D04B18F1-289B-454A-BBFE-F95B103FDFF9", "", "DA92D882-190A-4935-B212-6B369B764425", "Assesment", "Main", "", "", 1, "B4595B71-A68A-425C-A755-67D750BEA36A" );
            RockMigrationHelper.AddBlockAttributeValue( "B4595B71-A68A-425C-A755-67D750BEA36A", "1068B3D3-D55A-4ADD-903F-035F27B3F7FE", @"b21dcd49-ac35-4b2b-9857-75213209b643" ); // Badges
            RockMigrationHelper.AddBlockAttributeValue( "B4595B71-A68A-425C-A755-67D750BEA36A", "F7EA0170-277B-4D43-92D8-7CD5CE13E66D", @"False" ); // Display Country Code
            RockMigrationHelper.AddBlockAttributeValue( "B4595B71-A68A-425C-A755-67D750BEA36A", "6142B9DA-B237-4E16-8485-7F2FAE1C70F6", @"6ec89e41-8c1d-4435-b5cd-0f8d876379c6,8181dda4-e8a1-4d26-9ab1-e54ac4f34184" ); // Family Edit Page
            RockMigrationHelper.AddBlockAttributeValue( "B4595B71-A68A-425C-A755-67D750BEA36A", "11053FC1-85E4-4F32-BC02-A54A7278DE9B", @"False" ); // Display Middle Name
            RockMigrationHelper.AddBlockAttributeValue( "B4595B71-A68A-425C-A755-67D750BEA36A", "6F136BE0-408E-4226-9CFD-320EE6842EAA", @"fd92fe87-d749-40f6-8161-77cfd8431afb" ); // Home Page
        }

        #endregion

        public override void Down()
        {
        }

    }
}
