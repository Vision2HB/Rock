using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 4, "1.5.0" )]
    class AddPersonFamilyAttributes : Migration
    {
        public override void Up()
        {
            AddPersonAttributes();
            AddFamilyAttributes();
        }

        public override void Down()
        {
        }

        private void AddPersonAttributes()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "Assessment Attributes", "fa fa-file-text", "Used for attributes having to do with assessments", SystemGuid.Category.PERSON_ASSESSMENT_ATTRIBUTES );
            RockMigrationHelper.UpdatePersonAttributeCategory( "Care Center Attributes", "fa fa-university", "Used for attributes having to do with assessments", SystemGuid.Category.PERSON_CARECENTER_ATTRIBUTES );
            RockMigrationHelper.UpdatePersonAttributeCategory( "Care Center Clothing Specialty Items Attributes", "fa fa-shopping-cart", "Used for attributes that are a clothing specialty item", SystemGuid.Category.PERSON_CARECENTER_CLOTHING_SPECIALTY_ITEMS );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DEFINED_VALUE, SystemGuid.Category.PERSON_ASSESSMENT_ATTRIBUTES, "Employment Status", "EmploymentStatus", "", "", 1, "", SystemGuid.Attribute.PERSON_EMPLOYMENT_STATUS );
            Sql( string.Format( @"  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{0}')
                          DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{1}')
  
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'definedtype', CONVERT(varchar(10), @DefinedTypeId), newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'allowmultiple', 'False', newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'displaydescription', 'False', newid())", SystemGuid.DefinedType.EMPLOYMENT_STATUS, SystemGuid.Attribute.PERSON_EMPLOYMENT_STATUS ) );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, SystemGuid.Category.PERSON_ASSESSMENT_ATTRIBUTES, "Length of Employment", "LengthOfEmployment", "", "The length of employment", 2, "", SystemGuid.Attribute.PERSON_LENGTH_OF_EMPLOYMENT );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DEFINED_VALUE, SystemGuid.Category.PERSON_ASSESSMENT_ATTRIBUTES, "Insurance Status", "InsuranceStatus", "", "", 3, "", SystemGuid.Attribute.PERSON_INSURANCE_STATUS );
            Sql( string.Format( @"  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{0}')
                          DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{1}')
  
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'definedtype', CONVERT(varchar(10), @DefinedTypeId), newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'allowmultiple', 'False', newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'displaydescription', 'False', newid())", SystemGuid.DefinedType.INSURANCE_STATUS, SystemGuid.Attribute.PERSON_INSURANCE_STATUS ) );


            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.BOOLEAN, SystemGuid.Category.PERSON_ASSESSMENT_ATTRIBUTES, "Counseling Benevolence Given", "CounselingBenevolenceGiven", "", "Determines if counseling benevolence has been given.", 4, "", SystemGuid.Attribute.PERSON_COUNSELING_BENEVOLENCE );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DEFINED_VALUE, SystemGuid.Category.PERSON_CARECENTER_ATTRIBUTES, "Preferred Language", "PreferredLanguage", "", "", 5, SystemGuid.DefinedValue.LANGUAGE_ENGLISH, SystemGuid.Attribute.PERSON_PREFERRED_LANGUAGE );
            Sql( string.Format( @"  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{0}')
                          DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{1}')
  
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'definedtype', CONVERT(varchar(10), @DefinedTypeId), newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'allowmultiple', 'False', newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'displaydescription', 'False', newid())", SystemGuid.DefinedType.PREFERRED_LANGUAGE, SystemGuid.Attribute.PERSON_PREFERRED_LANGUAGE ) );


            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_CARECENTER_ATTRIBUTES, "Bike Reception", "BikeReception", "", "The date the person received a bike.", 7, "", SystemGuid.Attribute.PERSON_BIKE_RECEPTION );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_CARECENTER_ATTRIBUTES, "Computer Reception", "ComputerReception", "", "The date the person received a computer.", 8, "", SystemGuid.Attribute.PERSON_COMPUTER_RECEPTION );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.BOOLEAN, SystemGuid.Category.PERSON_CARECENTER_ATTRIBUTES, "Care Center Participant", "CareCenterParticipant", "", "Determines if the person participates in the Care Center.", 9, "", SystemGuid.Attribute.PERSON_CARECENTER_PARTICIPANT );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_ASSESSMENT_ATTRIBUTES, "Approved For Dental", "ApprovedForDental", "", "The date the person was approved for dental.", 10, "", SystemGuid.Attribute.PERSON_DENTAL_APPROVED );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_ASSESSMENT_ATTRIBUTES, "Approved For Vision", "ApprovedForVision", "", "The date the person was approved for vision.", 11, "", SystemGuid.Attribute.PERSON_VISION_APPROVED );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DEFINED_VALUE, SystemGuid.Category.PERSON_CARECENTER_ATTRIBUTES, "Child Validated", "ChildValidated", "", "", 12, "", SystemGuid.Attribute.PERSON_CHILD_VALIDATED );
            Sql( string.Format( @"  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{0}')
                          DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{1}')
  
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'definedtype', CONVERT(varchar(10), @DefinedTypeId), newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'allowmultiple', 'False', newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'displaydescription', 'False', newid())", SystemGuid.DefinedType.CHILD_VALIDATED, SystemGuid.Attribute.PERSON_CHILD_VALIDATED ) );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_CARECENTER_ATTRIBUTES, "Coat", "CoatReception", "", "The date the person last received a coat.", 13, "", SystemGuid.Attribute.PERSON_COAT_RECEPTION );
            Sql( string.Format( @" 
    DECLARE @AttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{0}' )
    DECLARE @CategoryId INT = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '{1}' )
    INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] ) VALUES ( @AttributeId, @CategoryId )
", SystemGuid.Attribute.PERSON_COAT_RECEPTION, SystemGuid.Category.PERSON_CARECENTER_CLOTHING_SPECIALTY_ITEMS ) );
        }

        private void AddFamilyAttributes()
        {
            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.INTEGER, "Household Size", "", 0, "", SystemGuid.Attribute.FAMILY_HOUSEHOLD_SIZE );
            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.CURRENCY, "Household Monthly Income", "The monthly household income of the family.", 1, "", SystemGuid.Attribute.FAMILY_HOUSEHOLD_INCOME );

            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.CURRENCY, "Scholarship Amount", "", 2, "", SystemGuid.Attribute.FAMILY_CLOTHING_SCHOLARSHIP_AMOUNT );

            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.DATE, "Food Grace Visit", "The date of the last grace food visit was given. A grace visit allows for someone to get a food visit if they are missing required paperwork.", 3, "", SystemGuid.Attribute.FAMILY_GRACE_FOOD_VISIT );
            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.DATE, "Food Courtesy Visit", "The date of the last courtesy food visit was given. A courtesy visit allows for someone to get a food visit if they are outside of the geographic boundaries.", 4, "", SystemGuid.Attribute.FAMILY_COURTESY_FOOD_VISIT );

            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.INTEGER, "Food Visits Per Month", "", 5, "1", SystemGuid.Attribute.FAMILY_FOOD_VISITS_PER_MONTH );

            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.DATE, "Food Visit Override Expire Date", "", 6, "", SystemGuid.Attribute.FAMILY_FOOD_VISITS_OVERRIDE_EXPIRE );

            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.DEFINED_VALUE, "Family Income Source", "", 7, "", SystemGuid.Attribute.FAMILY_INCOME_SOURCE );
            Sql( string.Format( @"  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{0}')
                          DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{1}')
  
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'definedtype', CONVERT(varchar(10), @DefinedTypeId), newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'allowmultiple', 'False', newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'displaydescription', 'False', newid())", SystemGuid.DefinedType.FAMILY_INCOME_SOURCE, SystemGuid.Attribute.FAMILY_INCOME_SOURCE ) );


            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.DEFINED_VALUE, "Church Status", "", 8, SystemGuid.DefinedValue.CHURCHSTATUS_UNKNOWN, SystemGuid.Attribute.FAMILY_CHURCH_STATUS );
            Sql( string.Format( @"  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{0}')
                          DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{1}')
  
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'definedtype', CONVERT(varchar(10), @DefinedTypeId), newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'allowmultiple', 'False', newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'displaydescription', 'False', newid())", SystemGuid.DefinedType.FAMILY_CHURCH_STATUS, SystemGuid.Attribute.FAMILY_CHURCH_STATUS ) );

            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.DEFINED_VALUE, "Duration Attended Willow", "", 9, "", SystemGuid.Attribute.FAMILY_DURATION_ATTENDED );
            Sql( string.Format( @"  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{0}')
                          DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{1}')
  
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'definedtype', CONVERT(varchar(10), @DefinedTypeId), newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'allowmultiple', 'False', newid())
                          INSERT INTO [AttributeQualifier] 
	                        ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                          VALUES
	                        (0, @AttributeId, 'displaydescription', 'False', newid())", SystemGuid.DefinedType.FAMILY_DURATION_ATTENDED, SystemGuid.Attribute.FAMILY_DURATION_ATTENDED ) );

            RockMigrationHelper.AddGroupTypeGroupAttribute( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, Rock.SystemGuid.FieldType.BOOLEAN, "Clothing Visit Override", "", 10, "", SystemGuid.Attribute.FAMILY_CLOTHING_VISIT_OVERRIDE );

        }
    }
}
