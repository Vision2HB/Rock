using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 3, "1.5.0" )]
    class AddGlobalAttributes : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( "5997C8D3-8840-4591-99A5-552919F90CBD", "Care Center", "fa fa-university", "Global Attributes for the Willow Care Center", SystemGuid.Category.GLOBAL_CARE_CENTER );

            // sql to fix bug? in update category
            Sql( string.Format( @"  UPDATE [Category] SET [EntityTypeQualifierColumn] = 'EntityTypeId' WHERE [Guid] = '{0}'", SystemGuid.Category.GLOBAL_CARE_CENTER ) );

            // add global attributes
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Resource Visit Enabled Count", "The number of food visits one must complete before the Resource Visit option is enabled.", 3, "3", SystemGuid.Attribute.GLOBAL_RESOURCE_VISIT_ENABLED_COUNT );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Default Food Visits Per Calendar Month", "The default number of food visits allowed in a calendar month.", 4, "1", SystemGuid.Attribute.GLOBAl_DEFAULT_FOOD_VISITS_PER_MONTH );

            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Car Max Age", "The number of years a car must be to be able to be eligible for repairs.", 5, "20", SystemGuid.Attribute.GLOBAL_CAR_MAX_AGE );
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Car Max Milage", "The max mileage a car can have to be eligible for repairs.", 6, "200000", SystemGuid.Attribute.GLOBAL_CAR_MAX_MILAGE );

            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Diaper Age", "The age in weeks that a child will be considered for diapers.", 7, "260", SystemGuid.Attribute.GLOBAL_DIAPER_AGE );

            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Birthday Cake Available Timespan", "The number of days +/- a person has to be to display the birthday cake option for food visit.", 8, "14", SystemGuid.Attribute.GLOBAL_BIRTHDAY_CAKE_TIMESPAN );

            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, "", "", "Ineligible Duration for No Show", "The number of days a person will be ineligible for scheduling a new appointment if they no show for an appointment.", 9, "365", SystemGuid.Attribute.GLOBAL_INELIGIBLE_DURATION_NO_SHOW );

            // add category to global attributes
            Sql( @"  INSERT INTO [AttributeCategory]
  ([AttributeId], [CategoryId])
  SELECT [Id], (SELECT [Id] FROM [Category] WHERE [Guid] = '0785DD67-C5A2-7C8A-42A6-97F8A470C02C')
  FROM [Attribute] WHERE [Guid] IN ('75EEC88A-2F4D-5ABA-43CF-08775C8AB555', 'ED10FC4C-4BC1-1F8A-48D7-02DA9DFBB60E', '6F9AF89A-22FE-43B4-4382-144325BC13AC', '051977B8-9488-C181-4F7E-3508B5638D13', 'DCF867F6-9C22-0F97-4383-0DCD9F928F29', '0288C1B4-2ACF-4D87-4B2A-4855F35AA28C', 'BBBBCAC9-A520-B2A4-437B-783921B1A683', '33DD6131-10A5-7686-48BC-6835A4E032E2', 'E1206D8F-9190-408A-4278-65E919966827', 'FCCBD10B-B408-BFA1-4940-083DF8293F75', '99D092CD-5AAC-C688-497D-8060C1FFEFCC')" );


            // Add Campus attribute for care center location
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Campus", "0A495222-23B7-41D3-82C8-D484CDB75D17", "", "", "Care Center Address", null, "Address of the campus care center.", 0, "", "B93DE189-90C4-4BD9-B2F0-8E12261C7260" );
        }

        public override void Down()
        {
        }
    }
}
