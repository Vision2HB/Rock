using System;
using Rock.Plugin;

namespace org.willowcreek.CareCenter.Migrations
{
    [MigrationNumber( 2, "1.5.0" )]
    class AddDefinedTypes : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( "6028D502-79F4-4A74-9323-525E90F900C7", "Care Center", "fa fa-university", "", SystemGuid.Category.DEFINEDTYPE_CARE_CENTER );

            RockMigrationHelper.AddDefinedType( "Care Center", "Poverty Guidlines", "The poverty tables for looking up if individuals meet the guidelines. Values are based off of household size.", SystemGuid.DefinedType.POVERTY_GUIDELINES );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.POVERTY_GUIDELINES, Rock.SystemGuid.FieldType.CURRENCY, "Non-Attendee Level", "NonAttendeeLevel", "The poverty level for those who do not attend the church.", 0, "", SystemGuid.Attribute.DEFINEDVALUE_POVERTY_NONATTENDER );
            Sql( "UPDATE [Attribute] SET [IsGridColumn] = 1 WHERE [Guid] = 'AB843262-7524-A88D-40A0-3046F40D7036' " );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.POVERTY_GUIDELINES, Rock.SystemGuid.FieldType.CURRENCY, "Attendee Level", "AttendeeLevel", "The poverty level for those who do attend the church.", 0, "", SystemGuid.Attribute.DEFINEDVALUE_POVERTY_ATTENDER );
            Sql( "UPDATE [Attribute] SET [IsGridColumn] = 1 WHERE [Guid] = '6DBD25C3-972F-B889-40BF-3C711297172A' " );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.POVERTY_GUIDELINES, "1", "Household Size 1", "50BF2D9C-ECCD-2796-4EB1-D9C6EFCE18EC", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "50BF2D9C-ECCD-2796-4EB1-D9C6EFCE18EC", "AB843262-7524-A88D-40A0-3046F40D7036", "980.83" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "50BF2D9C-ECCD-2796-4EB1-D9C6EFCE18EC", "6DBD25C3-972F-B889-40BF-3C711297172A", "1471.25" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.POVERTY_GUIDELINES, "2", "Household Size 2", "F539215B-F695-4C85-4356-7D521A203235", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F539215B-F695-4C85-4356-7D521A203235", "AB843262-7524-A88D-40A0-3046F40D7036", "1327.50" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F539215B-F695-4C85-4356-7D521A203235", "6DBD25C3-972F-B889-40BF-3C711297172A", "1991.25" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.POVERTY_GUIDELINES, "3", "Household Size 3", "D094AC09-06E6-52A1-4B62-6BE95FB1FC51", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D094AC09-06E6-52A1-4B62-6BE95FB1FC51", "AB843262-7524-A88D-40A0-3046F40D7036", "1674.17" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D094AC09-06E6-52A1-4B62-6BE95FB1FC51", "6DBD25C3-972F-B889-40BF-3C711297172A", "2511.25" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.POVERTY_GUIDELINES, "4", "Household Size 4", "B977774B-F729-419C-4640-B322AED27A10", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B977774B-F729-419C-4640-B322AED27A10", "AB843262-7524-A88D-40A0-3046F40D7036", "2020.83" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B977774B-F729-419C-4640-B322AED27A10", "6DBD25C3-972F-B889-40BF-3C711297172A", "3031.25" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.POVERTY_GUIDELINES, "5", "Household Size 5", "9DD540B0-8939-749B-4EBB-EF0DD1A857E6", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9DD540B0-8939-749B-4EBB-EF0DD1A857E6", "AB843262-7524-A88D-40A0-3046F40D7036", "2367.50" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9DD540B0-8939-749B-4EBB-EF0DD1A857E6", "6DBD25C3-972F-B889-40BF-3C711297172A", "3551.25" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.POVERTY_GUIDELINES, "6", "Household Size 6", "F4584C01-9721-778A-4F03-76A33D6395BE", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F4584C01-9721-778A-4F03-76A33D6395BE", "AB843262-7524-A88D-40A0-3046F40D7036", "2714.17" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F4584C01-9721-778A-4F03-76A33D6395BE", "6DBD25C3-972F-B889-40BF-3C711297172A", "4071.25" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.POVERTY_GUIDELINES, "7", "Household Size 7", "E1D9DBD3-7ACD-5EBF-4E48-3D9246837D31", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E1D9DBD3-7ACD-5EBF-4E48-3D9246837D31", "AB843262-7524-A88D-40A0-3046F40D7036", "3060.83" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E1D9DBD3-7ACD-5EBF-4E48-3D9246837D31", "6DBD25C3-972F-B889-40BF-3C711297172A", "4591.25" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.POVERTY_GUIDELINES, "8", "Household Size 8", "18681DE6-047A-10A1-4B75-E8803FD19105", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "18681DE6-047A-10A1-4B75-E8803FD19105", "AB843262-7524-A88D-40A0-3046F40D7036", "3407.50" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "18681DE6-047A-10A1-4B75-E8803FD19105", "6DBD25C3-972F-B889-40BF-3C711297172A", "5111.25" );

            RockMigrationHelper.AddDefinedType( "Care Center", "Visit Cancel Reason", "Reasons for canceling a Care Center visit.", SystemGuid.DefinedType.VISIT_CANCEL_REASON );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.VISIT_CANCEL_REASON, "Had To Leave", "", SystemGuid.DefinedValue.VISITCANCELREASON_HAD_TO_LEAVE, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.VISIT_CANCEL_REASON, "Left Without Service", "", SystemGuid.DefinedValue.VISITCANCELREASON_LEFT_WITHOUT_SERVICE, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.VISIT_CANCEL_REASON, "Service Area At Capacity", "", SystemGuid.DefinedValue.VISITCANCELREASON_SERVICE_AREA_AT_CAPACITY, false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Employment Status", "Care Center employment statuses.", SystemGuid.DefinedType.EMPLOYMENT_STATUS );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_STATUS, "Full-time", "", SystemGuid.DefinedValue.EMPLOYMENT_FULLTIME, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_STATUS, "Part-time", "", SystemGuid.DefinedValue.EMPLOYMENT_PARTTIME, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_STATUS, "Retired", "", SystemGuid.DefinedValue.EMPLOYMENT_RETIRED, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_STATUS, "Disability", "", SystemGuid.DefinedValue.EMPLOYMENT_DISABILITY, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_STATUS, "Unemployed", "", SystemGuid.DefinedValue.EMPLOYMENT_UNEMPLOYED, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_STATUS, "Other", "", SystemGuid.DefinedValue.EMPLOYMENT_OTHER, false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Housing Status", "Care Center housing statuses.", SystemGuid.DefinedType.HOUSING_STATUS );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.HOUSING_STATUS, "Own", "", SystemGuid.DefinedValue.HOUSING_OWN, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.HOUSING_STATUS, "Rent", "", SystemGuid.DefinedValue.HOUSING_RENT, false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Insurance Status", "Care Center insurance statuses.", SystemGuid.DefinedType.INSURANCE_STATUS );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INSURANCE_STATUS, "Has Insurance", "", SystemGuid.DefinedValue.INSURANCE_HAS, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INSURANCE_STATUS, "Medicaid", "", SystemGuid.DefinedValue.INSURANCE_MEDICAID, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INSURANCE_STATUS, "No Insurance", "", SystemGuid.DefinedValue.INSURANCE_NONE, false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Preferred Language", "Care Center preferred language.", SystemGuid.DefinedType.PREFERRED_LANGUAGE );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.PREFERRED_LANGUAGE, Rock.SystemGuid.FieldType.TEXT, "Abbreviation", "Abbreviation", "Abbreviation for the language.", 0, "", SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_ABBREVIATION );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.PREFERRED_LANGUAGE, Rock.SystemGuid.FieldType.TEXT, "Badge Color", "BadgeColor", "The badge color for the language.", 0, "", SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_BADGE_COLOR );
            Sql( "UPDATE [Attribute] SET [IsGridColumn] = 1 WHERE [Guid] = 'CBAC8153-ECE6-DDAA-4A58-E5DE558712F9' " );
            Sql( "UPDATE [Attribute] SET [IsGridColumn] = 1 WHERE [Guid] = '7C328CF0-B995-4DB7-48DC-00E025F49B37' " );

            RockMigrationHelper.AddDefinedType( "Care Center", "Job Participated In", "Care Center employment participated in.", SystemGuid.DefinedType.EMPLOYMENT_PARTICIPATED_IN);
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_PARTICIPATED_IN, "Workshops", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_PARTICIPATED_IN, "Hireways", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_PARTICIPATED_IN, "Job Fair", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_PARTICIPATED_IN, "Computer Lab", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_PARTICIPATED_IN, "EGAP", "" );

            RockMigrationHelper.AddDefinedType( "Care Center", "Job Classification", "Care Center employment job classifications.", SystemGuid.DefinedType.EMPLOYMENT_JOB_CLASSIFICATION );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_JOB_CLASSIFICATION, "Managerial/Executive", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_JOB_CLASSIFICATION, "Professional/Sales/Customer", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_JOB_CLASSIFICATION, "Office & Clerical", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_JOB_CLASSIFICATION, "Skilled Labor", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.EMPLOYMENT_JOB_CLASSIFICATION, "General Labor/Service Worker", "" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PREFERRED_LANGUAGE, "English", "", SystemGuid.DefinedValue.LANGUAGE_ENGLISH, false );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.LANGUAGE_ENGLISH, SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_ABBREVIATION, "ENG" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.LANGUAGE_ENGLISH, SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_BADGE_COLOR, "#597eaa" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PREFERRED_LANGUAGE, "Spanish", "", SystemGuid.DefinedValue.LANGUAGE_SPANISH, false );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.LANGUAGE_SPANISH, SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_ABBREVIATION, "ESP" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.LANGUAGE_SPANISH, SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_BADGE_COLOR, "#e06666" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PREFERRED_LANGUAGE, "Polish", "", SystemGuid.DefinedValue.LANGUAGE_POLISH, false );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.LANGUAGE_POLISH, SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_ABBREVIATION, "POL" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.LANGUAGE_POLISH, SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_BADGE_COLOR, "#e0a266" );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PREFERRED_LANGUAGE, "Other", "", SystemGuid.DefinedValue.LANGUAGE_OTHER, false );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.LANGUAGE_OTHER, SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_ABBREVIATION, "OTH" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.LANGUAGE_OTHER, SystemGuid.Attribute.DEFINEDVALUE_LANGUAGE_BADGE_COLOR, "#93c47d" );

            RockMigrationHelper.AddDefinedType( "Care Center", "Child Validation Types", "Various ways a child can be validated.", SystemGuid.DefinedType.CHILD_VALIDATED );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.CHILD_VALIDATED, "Medical Card", "", SystemGuid.DefinedValue.CHILDVALIDATION_MEDICAL_CARD, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.CHILD_VALIDATED, "Passport", "", SystemGuid.DefinedValue.CHILDVALIDATION_PASSPORT, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.CHILD_VALIDATED, "Court Document", "", SystemGuid.DefinedValue.CHILDVALIDATION_COURT_DOCUMENT, false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Income Source", "Determines where the income comes from.", SystemGuid.DefinedType.FAMILY_INCOME_SOURCE );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_INCOME_SOURCE, "Full-time", "", SystemGuid.DefinedValue.INCOMESOURCE_FULL_TIME, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_INCOME_SOURCE, "Part-time", "", SystemGuid.DefinedValue.INCOMESOURCE_PART_TIME, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_INCOME_SOURCE, "Retired", "", SystemGuid.DefinedValue.INCOMESOURCE_RETIRED, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_INCOME_SOURCE, "Unemployed", "", SystemGuid.DefinedValue.INCOMESOURCE_UNEMPLOYED, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_INCOME_SOURCE, "Other", "", SystemGuid.DefinedValue.INCOMESOURCE_OTHER, false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Church Status", "The church status of the family.", SystemGuid.DefinedType.FAMILY_CHURCH_STATUS );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_CHURCH_STATUS, "Unknown", "", SystemGuid.DefinedValue.CHURCHSTATUS_UNKNOWN, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_CHURCH_STATUS, "Attends Willow", "", SystemGuid.DefinedValue.CHURCHSTATUS_ATTENDS_WILLOW, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_CHURCH_STATUS, "Attends Different Church", "", SystemGuid.DefinedValue.CHURCHSTATUS_ATTENDS_DIFFERENT_CHURCH, false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_CHURCH_STATUS, "Does Not Attend Church", "", SystemGuid.DefinedValue.CHURCHSTATUS_DOES_NOT_ATTEND_CHURCH, false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Duration Attended", "The length of time family has attended Willow.", SystemGuid.DefinedType.FAMILY_DURATION_ATTENDED);
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_DURATION_ATTENDED, "0-5 m", "", "446F1990-CC8B-4DB6-82E0-75A90C46FC0E", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_DURATION_ATTENDED, "6-11 m", "", "D6DC3075-5698-485E-BDDA-2FBA7400B1FB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_DURATION_ATTENDED, "1-3 yr", "", "2BB2017C-1B70-4169-8E3F-DD48A44B45EC", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.FAMILY_DURATION_ATTENDED, "4+ yrs", "", "A4C75D86-59B1-4B40-990B-DD94A48C5F1D", true );

            RockMigrationHelper.AddDefinedType( "Care Center", "Food Visit Intake Options", "The options available on a Food Visit intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_FOOD );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INTAKE_OPTIONS_FOOD, "USDA Form Signed", "USDA Form Signed", "FFCF630A-5973-4447-A26E-507B5F5A2452", false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Bread Visit Intake Options", "The options available on a Bread Visit intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_BREAD );

            RockMigrationHelper.AddDefinedType( "Care Center", "Clothing Visit Intake Options", "The options available on a Clothing Visit intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_CLOTHING );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INTAKE_OPTIONS_CLOTHING, "Caris", "Caris", "7B0D1B4A-B903-48B7-AF63-B079FE70EF1C", false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INTAKE_OPTIONS_CLOTHING, "Fellowship Housing", "Fellowship Housing", "05BD069B-C2B5-42D7-BC12-BA4840260914", false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INTAKE_OPTIONS_CLOTHING, "Medical Card", "Has Medical Card", "E68C4002-5871-4BD5-B158-272C91A5DD7C", false );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INTAKE_OPTIONS_CLOTHING, "Safe Family", "Safe Family", "08FA3B14-7DEA-4114-A283-492A0D8A3DE3", true );

            RockMigrationHelper.AddDefinedType( "Care Center", "Limited Clothing Visit Intake Options", "The options available on a Limited Clothing intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_LIMITED_CLOTHING );

            RockMigrationHelper.AddDefinedType( "Care Center", "Care Team Visit Intake Options", "The options available on a Care Team Visit intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_CARE_TEAM );

            RockMigrationHelper.AddDefinedType( "Care Center", "Resource Visit Intake Options", "The options available on a Resource Visit intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_RESOURCE );

            RockMigrationHelper.AddDefinedType( "Care Center", "Legal Intake Options", "The options available on a Legal intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_LEGAL );
            //RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INTAKE_OPTIONS_LEGAL, "In Physical Danger", "Are you in physical danger?", "70A17701-117A-468D-9A48-95DA6FE63ED6", false );
            //RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INTAKE_OPTIONS_LEGAL, "Court Date", "Do you have a court date before next week?", "090ED166-A609-471C-8447-7F3A063DEFB4", false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Legal (Immigration) Intake Options", "The options available on a Legal (Immigration) intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_LEGAL_IMMIGRATION );

            RockMigrationHelper.AddDefinedType( "Care Center", "Employment Intake Options", "The options available on an Employment intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_EMPLOYMENT );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.INTAKE_OPTIONS_EMPLOYMENT, "Veteran", "Are you a veteran?", "0015BFC1-89F8-4B01-BC6B-6211C8C6CE22", false );

            RockMigrationHelper.AddDefinedType( "Care Center", "Financial Coaching Intake Options", "The options available on a Finacial Coaching intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_FINANCIAL_COACHING );

            //RockMigrationHelper.AddDefinedType( "Care Center", "Tax Prep Intake Options", "The options available on a Tax Prep intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_TAX_PREP );

            RockMigrationHelper.AddDefinedType( "Care Center", "Response Pastor Intake Options", "The options available on Response Pastor intake. Note: The Description will be displayed as an option during intake, and the Value for selected item will be included in the workflow's 'option' attribute value.", SystemGuid.DefinedType.INTAKE_OPTIONS_RESPONSE_PASTOR );

            RockMigrationHelper.AddDefinedType( "Care Center", "Response Pastor Nature of Case", "Response Pastor Nature of Case Options", SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Suicidal Ideation", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Conciliation Request", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Doctrinal / Spiritual Question", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Grief / Loss", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Addiction", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Marital Distress / Divorce", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Mental Health Concerns", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Benevolence Counseling Request", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Abuse of a Minor or Vulnerable Adult", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Domestic Abuse", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Counseling Referal", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Parenting Issue", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Prayer Request", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Relational Problem", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Hospital Visit", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Funeral Request", "" );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.RESPONSE_PASTOR_NATURE_OF_CASE, "Prayer Request", "" );

            // Add 'Care Center Guest' connection status
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Care Center Guest", "Is someone who was added by visiting the Care Center", org.willowcreek.CareCenter.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_CARE_CENTER_GUEST, false );

            // Add new cancel html button that does not cause validation
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.BUTTON_HTML, "Cancel", "Used for a Cancel button on the form that does not do form validation. Styled to use the Bootstrap default styling.", "383C04BA-EBD5-400B-B256-0D6360F517DC" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "383C04BA-EBD5-400B-B256-0D6360F517DC", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""return confirm('Are you sure you want to Cancel this workflow?');"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-default"">
    {{ ButtonText }}
</a>" );

        }

        public override void Down()
        {
        }
    }
}
