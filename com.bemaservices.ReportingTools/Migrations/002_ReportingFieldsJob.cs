// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Collections.Generic;
using Rock.Plugin;

namespace com.bemaservices.ReportingTools
{
    [MigrationNumber( 2, "1.10.1" )]
    public class ReportingFieldsJob : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "BEMA Reporting Fields", "fa fa-user", "", com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS );

            // Person Attribute "Reporting Fields Update Date"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"FE95430C-322D-4B67-9C77-DFD1D4408725", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Reporting Fields Update Date", @"Reporting Fields Update Date", @"ReportingFieldsUpdateDate", @"", @"", 13, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.REPORTING_FIELDS_UPDATE_DATE_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.REPORTING_FIELDS_UPDATE_DATE_ATTRIBUTE, @"datePickerControlType", @"Date Picker", @"7375FAA4-6C8F-43B1-A071-AE734DE48C27" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.REPORTING_FIELDS_UPDATE_DATE_ATTRIBUTE, @"displayCurrentOption", @"False", @"01C83231-872E-43BF-B211-3ADEE9C31DF0" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.REPORTING_FIELDS_UPDATE_DATE_ATTRIBUTE, @"displayDiff", @"False", @"485F87FD-532D-4A26-AAAA-50E0EDB6023C" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.REPORTING_FIELDS_UPDATE_DATE_ATTRIBUTE, @"format", @"", @"DE47CB91-C69A-498F-A28B-73A146CCAEEA" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.REPORTING_FIELDS_UPDATE_DATE_ATTRIBUTE, @"futureYearCount", @"", @"1B21B600-0EED-4E26-AA4A-FDF7177F247B" );

            // Person Attribute "Family - Head Of Household"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"1EDAFDED-DFE6-4334-B019-6EECBA89E05A", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Family - Head Of Household", @"", @"HeadOfHousehold", @"", @"", 216, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_HEAD_OF_HOUSEHOLD_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_HEAD_OF_HOUSEHOLD_ATTRIBUTE, @"falsetext", @"No", @"1F4ABF3B-C893-41F2-95CD-6DC19309E0B7" );

            // Person Attribute "Family - Full Name With Nicknames And No Titles"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Family - Full Name With Nicknames And No Titles", @"", @"FamilyFullNameWithNicknamesAndNoTitles", @"", @"", 235, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE, @"documentfolderroot", @"", @"58DDE480-A6EF-438C-9337-58C118A71C3F" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE, @"imagefolderroot", @"", @"7AB716C3-5236-431B-831A-7AD79299D250" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE, @"toolbar", @"Light", @"E468CBBB-5E27-47B4-9A6C-2AD64A073D88" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE, @"userspecificroot", @"False", @"0095E8E8-F2CA-4933-AE28-3DB9EAA36FEC" );

            // Person Attribute "Family - First Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Family - First Names", @"", @"FamilyFirstNames", @"", @"", 236, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FIRST_NAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FIRST_NAMES_ATTRIBUTE, @"documentfolderroot", @"", @"73C2EF70-BA89-4637-AF5D-032E767311D2" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FIRST_NAMES_ATTRIBUTE, @"imagefolderroot", @"", @"0FAB1838-F787-4BEC-BD34-ED80B280BB83" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FIRST_NAMES_ATTRIBUTE, @"toolbar", @"Light", @"DF74C3C5-1F75-416B-A82A-89E4E9524C70" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FIRST_NAMES_ATTRIBUTE, @"userspecificroot", @"False", @"B6EE0323-86E1-45CE-A078-22E55433B8D5" );

            // Person Attribute "Family - Full Name With Nick Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Family - Full Name With Nick Names", @"", @"FamilyFullNameWithNickNames", @"", @"", 237, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_ATTRIBUTE, @"documentfolderroot", @"", @"5B3864F2-F10A-4088-B2B4-3AADD3C9F355" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_ATTRIBUTE, @"imagefolderroot", @"", @"7EB94A52-59EA-4A22-BE91-70A826EC4421" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_ATTRIBUTE, @"toolbar", @"Light", @"727BA694-09BE-4E12-B44A-FA04A1E9AAA2" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_ATTRIBUTE, @"userspecificroot", @"False", @"2EC8E526-74B2-49C0-BCEE-4830557A3541" );

            // Person Attribute "Family - Last Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Family - Last Names", @"", @"FamilyLastNames", @"", @"", 238, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_LAST_NAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_LAST_NAMES_ATTRIBUTE, @"documentfolderroot", @"", @"D30EBF8E-C1E0-4548-9814-DAFC3CC0259E" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_LAST_NAMES_ATTRIBUTE, @"imagefolderroot", @"", @"DCFCD4C7-BFD8-4912-AEAF-36E306BD70D0" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_LAST_NAMES_ATTRIBUTE, @"toolbar", @"Light", @"A3C9A606-6B73-4DAB-8019-9EFB38EBC020" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_LAST_NAMES_ATTRIBUTE, @"userspecificroot", @"False", @"301E863E-B72D-4B53-BE49-58A6E6A6FB6B" );

            // Person Attribute "Family - Nick Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Family - Nick Names", @"", @"FamilyNickNames", @"", @"", 239, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_NICKNAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_NICKNAMES_ATTRIBUTE, @"documentfolderroot", @"", @"E2616A6B-072C-4B9A-B958-9B61CF72C7BA" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_NICKNAMES_ATTRIBUTE, @"imagefolderroot", @"", @"5DF9A256-32FB-4B4E-A135-C5A147C862BF" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_NICKNAMES_ATTRIBUTE, @"toolbar", @"Light", @"68B73648-D032-42F9-B047-99988167F266" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_NICKNAMES_ATTRIBUTE, @"userspecificroot", @"False", @"1FD0791B-93F1-4AB1-A639-D634D66384B8" );

            // Person Attribute "Family - Titles"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Family - Titles", @"", @"FamilyTitles", @"", @"", 240, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_TITLES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_TITLES_ATTRIBUTE, @"documentfolderroot", @"", @"933FFB57-8392-4E3B-9EC4-71C1400A7874" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_TITLES_ATTRIBUTE, @"imagefolderroot", @"", @"6C47856D-50F6-4D2E-963E-E2DC6130CB4D" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_TITLES_ATTRIBUTE, @"toolbar", @"Light", @"D8B32B35-5926-4B1A-8460-ED48900F5189" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_TITLES_ATTRIBUTE, @"userspecificroot", @"False", @"B4CB2405-F148-47C0-B8F6-93583450C858" );

            // Person Attribute "Family - Full Name With First Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Family - Full Name With First Names", @"", @"FamilyFullNameWithFirstNames", @"", @"", 241, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_FIRST_NAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_FIRST_NAMES_ATTRIBUTE, @"documentfolderroot", @"", @"DF46C735-6A1F-4EE0-B003-C8780B5BA96C" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_FIRST_NAMES_ATTRIBUTE, @"imagefolderroot", @"", @"491CE807-4CDF-400B-B188-122F824E82AF" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_FIRST_NAMES_ATTRIBUTE, @"toolbar", @"Light", @"967C22BF-AD87-44BC-AB03-29D7D6C1B63E" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_FIRST_NAMES_ATTRIBUTE, @"userspecificroot", @"False", @"F8F33560-5761-409E-8EB9-58BA08E61ACE" );

            // Person Attribute "Giving Unit - Head Of Household"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"1EDAFDED-DFE6-4334-B019-6EECBA89E05A", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Giving Unit - Head Of Household", @"", @"HeadOfHousehold", @"", @"", 216, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_HEAD_OF_HOUSEHOLD_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_HEAD_OF_HOUSEHOLD_ATTRIBUTE, @"falsetext", @"No", @"1F4ABF3B-C893-41F2-95CD-6DC19309E0B7" );

            // Person Attribute "Giving Unit - Full Name With Nicknames And No Titles"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Giving Unit - Full Name With Nicknames And No Titles", @"", @"GivingUnitFullNameWithNicknamesAndNoTitles", @"", @"", 235, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE, @"documentfolderroot", @"", @"58DDE480-A6EF-438C-9337-58C118A71C3F" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE, @"imagefolderroot", @"", @"7AB716C3-5236-431B-831A-7AD79299D250" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE, @"toolbar", @"Light", @"E468CBBB-5E27-47B4-9A6C-2AD64A073D88" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE, @"userspecificroot", @"False", @"0095E8E8-F2CA-4933-AE28-3DB9EAA36FEC" );

            // Person Attribute "Giving Unit - First Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Giving Unit - First Names", @"", @"GivingUnitFirstNames", @"", @"", 236, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FIRST_NAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FIRST_NAMES_ATTRIBUTE, @"documentfolderroot", @"", @"73C2EF70-BA89-4637-AF5D-032E767311D2" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FIRST_NAMES_ATTRIBUTE, @"imagefolderroot", @"", @"0FAB1838-F787-4BEC-BD34-ED80B280BB83" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FIRST_NAMES_ATTRIBUTE, @"toolbar", @"Light", @"DF74C3C5-1F75-416B-A82A-89E4E9524C70" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FIRST_NAMES_ATTRIBUTE, @"userspecificroot", @"False", @"B6EE0323-86E1-45CE-A078-22E55433B8D5" );

            // Person Attribute "Giving Unit - Full Name With Nick Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Giving Unit - Full Name With Nick Names", @"", @"GivingUnitFullNameWithNickNames", @"", @"", 237, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_ATTRIBUTE, @"documentfolderroot", @"", @"5B3864F2-F10A-4088-B2B4-3AADD3C9F355" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_ATTRIBUTE, @"imagefolderroot", @"", @"7EB94A52-59EA-4A22-BE91-70A826EC4421" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_ATTRIBUTE, @"toolbar", @"Light", @"727BA694-09BE-4E12-B44A-FA04A1E9AAA2" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_ATTRIBUTE, @"userspecificroot", @"False", @"2EC8E526-74B2-49C0-BCEE-4830557A3541" );

            // Person Attribute "Giving Unit - Last Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Giving Unit - Last Names", @"", @"GivingUnitLastNames", @"", @"", 238, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_LAST_NAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_LAST_NAMES_ATTRIBUTE, @"documentfolderroot", @"", @"D30EBF8E-C1E0-4548-9814-DAFC3CC0259E" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_LAST_NAMES_ATTRIBUTE, @"imagefolderroot", @"", @"DCFCD4C7-BFD8-4912-AEAF-36E306BD70D0" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_LAST_NAMES_ATTRIBUTE, @"toolbar", @"Light", @"A3C9A606-6B73-4DAB-8019-9EFB38EBC020" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_LAST_NAMES_ATTRIBUTE, @"userspecificroot", @"False", @"301E863E-B72D-4B53-BE49-58A6E6A6FB6B" );

            // Person Attribute "Giving Unit - Nick Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Giving Unit - Nick Names", @"", @"GivingUnitNickNames", @"", @"", 239, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_NICKNAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_NICKNAMES_ATTRIBUTE, @"documentfolderroot", @"", @"E2616A6B-072C-4B9A-B958-9B61CF72C7BA" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_NICKNAMES_ATTRIBUTE, @"imagefolderroot", @"", @"5DF9A256-32FB-4B4E-A135-C5A147C862BF" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_NICKNAMES_ATTRIBUTE, @"toolbar", @"Light", @"68B73648-D032-42F9-B047-99988167F266" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_NICKNAMES_ATTRIBUTE, @"userspecificroot", @"False", @"1FD0791B-93F1-4AB1-A639-D634D66384B8" );

            // Person Attribute "Giving Unit - Titles"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Giving Unit - Titles", @"", @"GivingUnitTitles", @"", @"", 240, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_TITLES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_TITLES_ATTRIBUTE, @"documentfolderroot", @"", @"933FFB57-8392-4E3B-9EC4-71C1400A7874" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_TITLES_ATTRIBUTE, @"imagefolderroot", @"", @"6C47856D-50F6-4D2E-963E-E2DC6130CB4D" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_TITLES_ATTRIBUTE, @"toolbar", @"Light", @"D8B32B35-5926-4B1A-8460-ED48900F5189" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_TITLES_ATTRIBUTE, @"userspecificroot", @"False", @"B4CB2405-F148-47C0-B8F6-93583450C858" );

            // Person Attribute "Giving Unit - Full Name With First Names"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", new List<string>( new string[] { com.bemaservices.ReportingTools.SystemGuid.Category.BEMA_REPORTING_FIELDS } ), @"Giving Unit - Full Name With First Names", @"", @"GivingUnitFullNameWithFirstNames", @"", @"", 241, @"", com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_FIRST_NAMES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_FIRST_NAMES_ATTRIBUTE, @"documentfolderroot", @"", @"DF46C735-6A1F-4EE0-B003-C8780B5BA96C" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_FIRST_NAMES_ATTRIBUTE, @"imagefolderroot", @"", @"491CE807-4CDF-400B-B188-122F824E82AF" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_FIRST_NAMES_ATTRIBUTE, @"toolbar", @"Light", @"967C22BF-AD87-44BC-AB03-29D7D6C1B63E" );
            RockMigrationHelper.AddAttributeQualifier( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_FIRST_NAMES_ATTRIBUTE, @"userspecificroot", @"False", @"F8F33560-5761-409E-8EB9-58BA08E61ACE" );
           
            // Page: Person Attributes         
            // Add Block to Page: Person Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "Family Names", "SectionB1", "", "", 2, "D713CB42-6355-4CF2-B7A1-51EA8473821F" );
            // Attrib Value for Block:Family Names, Attribute:Category Page: Person Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D713CB42-6355-4CF2-B7A1-51EA8473821F", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"326156e6-6a57-48bb-9e74-779ae3f95d43" );
            // Attrib Value for Block:Family Names, Attribute:Attribute Order Page: Person Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D713CB42-6355-4CF2-B7A1-51EA8473821F", "235C6D48-E1D1-410C-8006-1EA412BC12EF", @"" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}

