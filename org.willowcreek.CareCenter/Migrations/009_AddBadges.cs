using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 9, "1.5.0" )]
    class AddBadges : Migration
    {
        public override void Up()
        {
            // PERSON PROFILE

            int badgeOrder = 0;

            // banned badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.CareCenterBanBadge", "Care Center Banned Badge", "org.willowcreek.CareCenter.PersonProfile.Badge.CareCenterBanBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "C18B3B96-FD08-3ABA-4C6E-CD15E9DF3C83" );
            RockMigrationHelper.UpdatePersonBadge( "Care Center Banned Badge", "Shows if someone is banned from the Care Center.", "org.willowcreek.CareCenter.PersonProfile.Badge.CareCenterBanBadge", badgeOrder++, "5A1627DB-28A3-FBB7-44D4-B3CF6A87070B" );

            // link to Rock profile
            RockMigrationHelper.UpdatePersonBadge( "Rock Profile Link", "Provide Link from Care Center profile to Rock profile", "Rock.PersonProfile.Badge.Liquid", 0, "3DC1305D-23C4-40C1-8B70-BC0271ACA2D8" );

            Sql( @"
    DECLARE @LavaAttributeId int = (SELECT TOP 1 [ID] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' )
    DECLARE @BadgeEntityId int = (SELECT TOP 1 [Id] FROM [PersonBadge] WHERE [Guid] = '3DC1305D-23C4-40C1-8B70-BC0271ACA2D8' )

    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
    VALUES (0, @LavaAttributeId, @BadgeEntityId, '<a href=''/person/{{ Person.Id }}?SiteId=1'' class=''label label-default''>Rock Profile</a>', newid())
" );

            RockMigrationHelper.AddBlockAttributeValue( "4E397A53-2E0E-4419-B6A7-F46CCC807A31", "8E11F65B-7272-4E9F-A4F1-89CE08E658DE", "5a1627db-28a3-fbb7-44d4-b3cf6a87070b,3dc1305d-23c4-40c1-8b70-bc0271aca2d8", false );

            // LEFT SIDE

            // visit count badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.VisitCountBadge", "Care Center Visit Count", "org.willowcreek.CareCenter.PersonProfile.Badge.VisitCountBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "601A163B-3ECF-2388-423B-94861CFEE976" );
            RockMigrationHelper.UpdatePersonBadge( "Care Center Visitor Count", "Shows the number of times a person has visited the Care Center.", "org.willowcreek.CareCenter.PersonProfile.Badge.VisitCountBadge", badgeOrder++, "66BAB877-8F48-B38E-473C-63B62697A339" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "BE30C279-8084-453E-9CEC-D3838B4F5CD6", "F5AB231E-3836-4D52-BD03-BF79773C237A", "66BAB877-8F48-B38E-473C-63B62697A339", false );

            // food status badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.FoodVisitBadge", "Care Center Food Visit Status", "org.willowcreek.CareCenter.PersonProfile.Badge.FoodVisitBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "D5D20977-4EAD-E2B0-4A1B-87C2FBD520BB" );
            RockMigrationHelper.UpdatePersonBadge( "Care Center Food Visit Status", "Determines the status of the person's food visits.", "org.willowcreek.CareCenter.PersonProfile.Badge.FoodVisitBadge", badgeOrder++, "B85492F0-AC72-AFA3-40DB-0DAEA9377E32" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "BE30C279-8084-453E-9CEC-D3838B4F5CD6", "F5AB231E-3836-4D52-BD03-BF79773C237A", "B85492F0-AC72-AFA3-40DB-0DAEA9377E32", true );

            // clothing status badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.ClothingVisitBadge", "Care Center Clothing Visit Status", "org.willowcreek.CareCenter.PersonProfile.Badge.ClothingVisitBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "0633598C-5739-ECB2-479A-B2EABAC35335" );
            RockMigrationHelper.UpdatePersonBadge( "Care Center Clothing Visit Status", "Determines the status of the person's clothing visits.", "org.willowcreek.CareCenter.PersonProfile.Badge.ClothingVisitBadge", badgeOrder++, "0A71F46D-E8C6-E49E-4971-8040F006D735" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "BE30C279-8084-453E-9CEC-D3838B4F5CD6", "F5AB231E-3836-4D52-BD03-BF79773C237A", "0A71F46D-E8C6-E49E-4971-8040F006D735", true );

            // vision badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.VisionVisitBadge", "Care Center Vision Badge", "org.willowcreek.CareCenter.PersonProfile.Badge.VisionVisitBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "2C87A121-D339-D282-4AD4-FC61474C91AD" );
            RockMigrationHelper.UpdatePersonBadge( "Care Center Vision", "Shows if the person is approved for vision services. ", "org.willowcreek.CareCenter.PersonProfile.Badge.VisionVisitBadge", badgeOrder++, "2E265396-B124-7DA0-471C-B788A5AE88F9" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "BE30C279-8084-453E-9CEC-D3838B4F5CD6", "F5AB231E-3836-4D52-BD03-BF79773C237A", "2E265396-B124-7DA0-471C-B788A5AE88F9", true );

            // dental badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.DentalVisitBadge", "Care Center Dental Badge", "org.willowcreek.CareCenter.PersonProfile.Badge.DentalVisitBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "DF0E4085-F69C-BA9D-439D-7775A4C461DA" );
            RockMigrationHelper.UpdatePersonBadge( "Care Center Dental", "Shows if the person is approved for dental services. ", "org.willowcreek.CareCenter.PersonProfile.Badge.DentalVisitBadge", badgeOrder++, "E44529C1-F483-F6BF-47B3-AB7A9B245E49" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "BE30C279-8084-453E-9CEC-D3838B4F5CD6", "F5AB231E-3836-4D52-BD03-BF79773C237A", "E44529C1-F483-F6BF-47B3-AB7A9B245E49", true );

            // RIGHT SIDE

            // protection badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.CareCenterProtectionBadge", "Care Center Protection Badge", "org.willowcreek.CareCenter.PersonProfile.Badge.CareCenterProtectionBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "251AEA0B-661A-4AC0-A3A0-D8ED5A15A21E" );
            RockMigrationHelper.UpdatePersonBadge( "Care Center Protection Badge", "Displays an alert if person has an attendance restriction or warning or has been banned from the care center.", "org.willowcreek.CareCenter.PersonProfile.Badge.CareCenterProtectionBadge", badgeOrder++, "3ABC4E4A-3CAF-4C8E-9281-E57676FE7D7A" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "C1915885-3046-4EDE-BE0A-EEE8A47CFF75", "F5AB231E-3836-4D52-BD03-BF79773C237A", "3ABC4E4A-3CAF-4C8E-9281-E57676FE7D7A", false );

            // care team badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.CareTeamBadge", "Care Team Badge", "org.willowcreek.CareCenter.PersonProfile.Badge.CareTeamBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "5704DA23-C960-95B6-4025-733EBA232DEE" );
            RockMigrationHelper.UpdatePersonBadge( "Care Team Badge", "Shows if the person has had a visit with the Care Team.", "org.willowcreek.CareCenter.PersonProfile.Badge.CareTeamBadge", badgeOrder++, "831E8DAA-3350-E683-4999-3B2B29DCE715" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "C1915885-3046-4EDE-BE0A-EEE8A47CFF75", "F5AB231E-3836-4D52-BD03-BF79773C237A", "831E8DAA-3350-E683-4999-3B2B29DCE715", true );

            // income status badge
            RockMigrationHelper.UpdateEntityType( "org.willowcreek.CareCenter.PersonProfile.Badge.IncomeStatusBadge", "Care Center Income Status", "org.willowcreek.CareCenter.PersonProfile.Badge.IncomeStatusBadge, org.willowcreek.CareCenter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "E4266655-6381-2F8C-4E97-0F227111D5A5" );
            RockMigrationHelper.UpdatePersonBadge( "Care Center Income Status", "Determines if the person meets the income status. This processes looks at the 'Household Income' family attribute of each family the person is in. If not value is found the badge is shown as unknown. If the family the family has a 'Household Size' family attribute of that family and the 'Church Status'. If no household size value is found here then it uses the number of members in the family group. Using the family size the poverty guideline is looked up using the 'Poverty Guidelines' defined type.", "org.willowcreek.CareCenter.PersonProfile.Badge.IncomeStatusBadge", badgeOrder++, "2703820C-87B4-B497-4C79-0247D351E4B7" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "C1915885-3046-4EDE-BE0A-EEE8A47CFF75", "F5AB231E-3836-4D52-BD03-BF79773C237A", "2703820C-87B4-B497-4C79-0247D351E4B7", true );

            // preffered language badge
            RockMigrationHelper.UpdatePersonBadge( "Care Center Preferred Language", "Shows the preferred language for the person. ", "Rock.PersonProfile.Badge.Liquid", badgeOrder++, "07575E4C-7E5B-2397-4BAC-18AA33D7A809" );
            // add to badge group right
            RockMigrationHelper.AddBlockAttributeValue( "C1915885-3046-4EDE-BE0A-EEE8A47CFF75", "F5AB231E-3836-4D52-BD03-BF79773C237A", "07575E4C-7E5B-2397-4BAC-18AA33D7A809", true );

            Sql( @"DECLARE @LavaAttributeId int = (SELECT TOP 1 [ID] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' )
  DECLARE @BadgeEntityId int = (SELECT TOP 1 [Id] FROM [PersonBadge] WHERE [Guid] = '07575E4C-7E5B-2397-4BAC-18AA33D7A809' )

  INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId])
                VALUES
	                (0, @LavaAttributeId, @BadgeEntityId, '{% assign preferedLanguage = Person | Attribute:''PreferredLanguage'',''Object'' %}

{% assign abbreviation = preferedLanguage | Attribute:''Abbreviation'' %}
{% assign badgeColor = preferedLanguage | Attribute:''BadgeColor'' %}

<div class=""badge badge-language {{ abbreviation }}"" style=""color: {{ badgeColor }};"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName }} prefers {{ preferedLanguage.Value }}."">
    <div class=""badge-details"">
        <span>{{ abbreviation }}</span>
    </div>
</div>', newid(), getdate(), getdate(), null,null)" );

            // family size
            RockMigrationHelper.UpdatePersonBadge( "Care Center Household Size", "Shows the size of the household. ", "Rock.PersonProfile.Badge.Liquid", badgeOrder++, "E9A13966-F378-E695-484F-C5A1E6BFB9E1" );
            // add to badge group right
            RockMigrationHelper.AddBlockAttributeValue( "C1915885-3046-4EDE-BE0A-EEE8A47CFF75", "F5AB231E-3836-4D52-BD03-BF79773C237A", "E9A13966-F378-E695-484F-C5A1E6BFB9E1", true );

            Sql( @"DECLARE @LavaAttributeId int = (SELECT TOP 1 [ID] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' )
  DECLARE @BadgeEntityId int = (SELECT TOP 1 [Id] FROM [PersonBadge] WHERE [Guid] = 'E9A13966-F378-E695-484F-C5A1E6BFB9E1' )

  INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId])
                VALUES
	                (0, @LavaAttributeId, @BadgeEntityId, '{% assign familyMember = Person | Groups: ''10'',''All'' | First %}
{% assign family = familyMember.Group %}
{% assign familySize = family | Attribute:''HouseholdSize'' %}

{% if (familySize == '''') %}
    {% assign familySize = family.Members | Size %}
{% endif %}

<div class=""badge badge-householdsize"" data-toggle=""tooltip"" data-original-title=""There are {{ familySize }} individuals in the {{ Person.LastName}} family."">
    <div class=""badge-details"">
        <i class=""fa fa-home""></i>
        <span>{{ familySize }}</span>
    </div>
</div>', newid(), getdate(), getdate(), null,null)" );


            // church status
            RockMigrationHelper.UpdatePersonBadge( "Care Center Church Status", "Shows the person's status in church attendance. ", "Rock.PersonProfile.Badge.Liquid", badgeOrder++, "F8CD244F-49EC-9CA3-4C7B-6BBC8D4DC51D" );
            // add to badge group left
            RockMigrationHelper.AddBlockAttributeValue( "C1915885-3046-4EDE-BE0A-EEE8A47CFF75", "F5AB231E-3836-4D52-BD03-BF79773C237A", "F8CD244F-49EC-9CA3-4C7B-6BBC8D4DC51D", true );

            Sql( @"DECLARE @LavaAttributeId int = (SELECT TOP 1 [ID] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' )
  DECLARE @BadgeEntityId int = (SELECT TOP 1 [Id] FROM [PersonBadge] WHERE [Guid] = 'F8CD244F-49EC-9CA3-4C7B-6BBC8D4DC51D' )

  INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId])
                VALUES
	                (0, @LavaAttributeId, @BadgeEntityId, '{% assign familyMember = Person | Groups: ""10"",''All'' | First %}
{% assign family = familyMember.Group %}
{% assign churchStatus = family | Attribute:''ChurchStatus'',''RawValue'' | Upcase %}

{% case churchStatus %}
{% when ''9C4F8CDD-3FD6-67BF-499D-9A50223AED20'' %}
    <div class=""badge badge-churchstatus unknown"" data-toggle=""tooltip"" data-original-title=""Unknown church attendance."">
        <div class=""badge-details"">
            <span>W</span>
        </div>
    </div>
{% when ''0ED4BF39-4F07-A189-48F2-54401027A900'' %}
    <div class=""badge badge-churchstatus attends-willow"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName }} attends Willow Creek."">
        <div class=""badge-details"">
            <span>W</span>
        </div>
    </div>
{% when ''1C3EC090-8AAD-C9AF-4BF7-C4C2ABA7296C'' %}
    <div class=""badge badge-churchstatus different-church"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName }} attends a different church."">
        <div class=""badge-details"">
            <span>W</span>
        </div>
    </div>
{% when ''03BC9FC1-9E42-5AAF-4393-5CADE004AE51'' %}
    <div class=""badge badge-churchstatus no-church"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName }} does not attend a church."">
        <div class=""badge-details"">
            <span>W</span>
        </div>
    </div>
{% endcase %}', newid(), getdate(), getdate(), null,null)" );

        }

        public override void Down()
        {
        }
    }
}
