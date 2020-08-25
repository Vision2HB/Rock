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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace com.bemaservices.ReportingTools.Jobs
{
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (3600). Note, some operations could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", false, 60 * 60, "General", 1, "CommandTimeout" )]
    [DisallowConcurrentExecution]
    public class CalculateReportingFields : IJob
    {
        private const string SOURCE_OF_CHANGE = "Calculate Reporting Fields Job";

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculateReportingFields()
        {
        }

        /// <summary>
        /// Job that will run lava code on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            var familyType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var adultRole = familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

            var reportingFieldsUpdateDateAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.REPORTING_FIELDS_UPDATE_DATE_ATTRIBUTE.AsGuid() );

            var familyHeadOfHouseholdAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_HEAD_OF_HOUSEHOLD_ATTRIBUTE.AsGuid() );
            var familyFullNameNickNamesNoTitlesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE.AsGuid() );
            var familyFirstNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FIRST_NAMES_ATTRIBUTE.AsGuid() );
            var familyFullNameNickNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_NICKNAMES_ATTRIBUTE.AsGuid() );
            var familyLastNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_LAST_NAMES_ATTRIBUTE.AsGuid() );
            var familyNickNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_NICKNAMES_ATTRIBUTE.AsGuid() );
            var familyTitlesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_TITLES_ATTRIBUTE.AsGuid() );
            var familyFullNameFirstNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.FAMILY_FULL_NAME_FIRST_NAMES_ATTRIBUTE.AsGuid() );

            var givingUnitHeadOfHouseholdAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_HEAD_OF_HOUSEHOLD_ATTRIBUTE.AsGuid() );
            var givingUnitFullNameNickNamesNoTitlesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_NO_TITLES_ATTRIBUTE.AsGuid() );
            var givingUnitFirstNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FIRST_NAMES_ATTRIBUTE.AsGuid() );
            var givingUnitFullNameNickNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_NICKNAMES_ATTRIBUTE.AsGuid() );
            var givingUnitLastNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_LAST_NAMES_ATTRIBUTE.AsGuid() );
            var givingUnitNickNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_NICKNAMES_ATTRIBUTE.AsGuid() );
            var givingUnitTitlesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_TITLES_ATTRIBUTE.AsGuid() );
            var givingUnitFullNameFirstNamesAttribute = AttributeCache.Get( com.bemaservices.ReportingTools.SystemGuid.Attribute.GIVINGUNIT_FULL_NAME_FIRST_NAMES_ATTRIBUTE.AsGuid() );

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            int commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            var resultContext = new RockContext();
            resultContext.Database.CommandTimeout = commandTimeout;
            context.UpdateLastStatusMessage( "Getting Reporting Fields Dataset..." );
            var results = resultContext.Database.SqlQuery<FamilyNameResult>( "BEMA_ReportingTools_sp_ReportingFieldsDataset" ).ToList();

            int progressPosition = 0;
            int progressTotal = results.Count;

            foreach ( var result in results )
            {
                progressPosition++;
                // create new rock context for each person (https://weblog.west-wind.com/posts/2014/Dec/21/Gotcha-Entity-Framework-gets-slow-in-long-Iteration-Loops)
                //  RockContext updateContext = new RockContext();
                //updateContext.SourceOfChange = SOURCE_OF_CHANGE;
                // updateContext.Database.CommandTimeout = commandTimeout;
                // var attributeValueService = new AttributeValueService( updateContext );

                SaveAttributeSql( familyHeadOfHouseholdAttribute, result.Id, result.FamilyHeadofHousehold, commandTimeout );
                SaveAttributeSql( familyFullNameNickNamesNoTitlesAttribute, result.Id, result.FamilyFullNameNickNameNoTitle, commandTimeout );
                SaveAttributeSql( familyFirstNamesAttribute, result.Id, result.FamilyFirstName, commandTimeout );
                SaveAttributeSql( familyFullNameNickNamesAttribute, result.Id, result.FamilyFullNameNickName, commandTimeout );
                SaveAttributeSql( familyLastNamesAttribute, result.Id, result.FamilyLastNames, commandTimeout );
                SaveAttributeSql( familyNickNamesAttribute, result.Id, result.FamilyNickNames, commandTimeout );
                SaveAttributeSql( familyTitlesAttribute, result.Id, result.FamilyTitles, commandTimeout );
                SaveAttributeSql( familyFullNameFirstNamesAttribute, result.Id, result.FamilyFullNameFirstName, commandTimeout );

                SaveAttributeSql( givingUnitHeadOfHouseholdAttribute, result.Id, result.GivingUnitHeadofHousehold, commandTimeout );
                SaveAttributeSql( givingUnitFullNameNickNamesNoTitlesAttribute, result.Id, result.GivingUnitFullNameNickNameNoTitle, commandTimeout );
                SaveAttributeSql( givingUnitFirstNamesAttribute, result.Id, result.GivingUnitFirstName, commandTimeout );
                SaveAttributeSql( givingUnitFullNameNickNamesAttribute, result.Id, result.GivingUnitFullNameNickName, commandTimeout );
                SaveAttributeSql( givingUnitLastNamesAttribute, result.Id, result.GivingUnitLastNames, commandTimeout );
                SaveAttributeSql( givingUnitNickNamesAttribute, result.Id, result.GivingUnitNickNames, commandTimeout );
                SaveAttributeSql( givingUnitTitlesAttribute, result.Id, result.GivingUnitTitles, commandTimeout );
                SaveAttributeSql( givingUnitFullNameFirstNamesAttribute, result.Id, result.GivingUnitFullNameFirstName, commandTimeout );

                SaveAttributeSql( reportingFieldsUpdateDateAttribute, result.Id, RockDateTime.Now.ToString(), commandTimeout );
                // update stats
                context.UpdateLastStatusMessage( $"Updating Family Names {progressPosition} of {progressTotal}" );
            }

            context.UpdateLastStatusMessage( "" );
        }

        private static void SaveAttributeSql( AttributeCache attribute, int personId, string value, int commandTimeout )
        {
            try
            {
                if ( attribute != null )
                {
                    var query = string.Format( @"

                        DECLARE @PersonId int = {0}

                        DECLARE @AttributeId int = {1}

                        IF @PersonId IS NOT NULL AND @AttributeId IS NOT NULL
                        BEGIN

                            DECLARE @TheValue NVARCHAR(MAX) = '{2}'

                            -- Delete existing attribute value first (might have been created by Rock system)
                            DELETE [AttributeValue]
                            WHERE [AttributeId] = @AttributeId
                            AND [EntityId] = @PersonId

                            INSERT INTO [AttributeValue] (
                                [IsSystem],[AttributeId],[EntityId],
                                [Value],
                                [Guid])
                            VALUES(
                                1,@AttributeId,@PersonId,
                                @TheValue,
                                NEWID())
                        END
                        ",
                           personId,
                           attribute.Id,
                           value.Replace( "'", "''" )
                       );

                    int rows = DbService.ExecuteCommand( query, commandTimeout: commandTimeout );
                    if ( rows < 0 )
                    {
                        rows = 0;
                    }
                }
            }
            catch ( Exception ex )
            {
                string message = String.Format( "Error occurred saving {0} for Person with ID {1}", attribute.Name, personId );
                ExceptionLogService.LogException( new Exception( message, ex ) );
            }
        }

        public class FamilyNameResult
        {
            [Key]
            public int Id { get; set; }
            public int GroupId { get; set; }
            public string FamilyHeadofHousehold { get; set; }
            public string GivingUnitHeadofHousehold { get; set; }
            public string FamilyFullNameNickNameNoTitle { get; set; }
            public string GivingUnitFullNameNickNameNoTitle { get; set; }
            public string FamilyFirstName { get; set; }
            public string GivingUnitFirstName { get; set; }
            public string FamilyFullNameNickName { get; set; }
            public string GivingUnitFullNameNickName { get; set; }
            public string FamilyLastNames { get; set; }
            public string GivingUnitLastNames { get; set; }
            public string FamilyNickNames { get; set; }
            public string GivingUnitNickNames { get; set; }
            public string FamilyTitles { get; set; }
            public string GivingUnitTitles { get; set; }
            public string FamilyFullNameFirstName { get; set; }
            public string GivingUnitFullNameFirstName { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="EraResult"/> class.
            /// </summary>
            public FamilyNameResult() { }

        }
    }
}
