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

            int personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
            int attributeEntityTypeId = EntityTypeCache.Get( "Rock.Model.Attribute" ).Id;

            int progressPosition = 0;
            int progressTotal = results.Count;

            foreach ( var result in results )
            {
                progressPosition++;
                // create new rock context for each person (https://weblog.west-wind.com/posts/2014/Dec/21/Gotcha-Entity-Framework-gets-slow-in-long-Iteration-Loops)
                RockContext updateContext = new RockContext();
                updateContext.SourceOfChange = SOURCE_OF_CHANGE;
                updateContext.Database.CommandTimeout = commandTimeout;
                var attributeValueService = new AttributeValueService( updateContext );

                SaveAttribute( familyHeadOfHouseholdAttribute, result.PersonId, result.FamilyHeadofHousehold, updateContext, attributeValueService );
                SaveAttribute( familyFullNameNickNamesNoTitlesAttribute, result.PersonId, result.FamilyFullNameNickNameNoTitle, updateContext, attributeValueService );
                SaveAttribute( familyFirstNamesAttribute, result.PersonId, result.FamilyFirstName, updateContext, attributeValueService );
                SaveAttribute( familyFullNameNickNamesAttribute, result.PersonId, result.FamilyFullNameNickName, updateContext, attributeValueService );
                SaveAttribute( familyLastNamesAttribute, result.PersonId, result.FamilyLastNames, updateContext, attributeValueService );
                SaveAttribute( familyNickNamesAttribute, result.PersonId, result.FamilyNickNames, updateContext, attributeValueService );
                SaveAttribute( familyTitlesAttribute, result.PersonId, result.FamilyTitles, updateContext, attributeValueService );
                SaveAttribute( familyFullNameFirstNamesAttribute, result.PersonId, result.FamilyFullNameFirstName, updateContext, attributeValueService );

                SaveAttribute( givingUnitHeadOfHouseholdAttribute, result.PersonId, result.GivingUnitHeadofHousehold, updateContext, attributeValueService );
                SaveAttribute( givingUnitFullNameNickNamesNoTitlesAttribute, result.PersonId, result.GivingUnitFullNameNickNameNoTitle, updateContext, attributeValueService );
                SaveAttribute( givingUnitFirstNamesAttribute, result.PersonId, result.GivingUnitFirstName, updateContext, attributeValueService );
                SaveAttribute( givingUnitFullNameNickNamesAttribute, result.PersonId, result.GivingUnitFullNameNickName, updateContext, attributeValueService );
                SaveAttribute( givingUnitLastNamesAttribute, result.PersonId, result.GivingUnitLastNames, updateContext, attributeValueService );
                SaveAttribute( givingUnitNickNamesAttribute, result.PersonId, result.GivingUnitNickNames, updateContext, attributeValueService );
                SaveAttribute( givingUnitTitlesAttribute, result.PersonId, result.GivingUnitTitles, updateContext, attributeValueService );
                SaveAttribute( givingUnitFullNameFirstNamesAttribute, result.PersonId, result.GivingUnitFullNameFirstName, updateContext, attributeValueService );

                // update stats
                context.UpdateLastStatusMessage( $"Updating Family Names {progressPosition} of {progressTotal}" );
            }

            context.UpdateLastStatusMessage( "" );
        }

        private static void SaveAttribute( AttributeCache attribute, int personId, string value, RockContext updateContext, AttributeValueService attributeValueService )
        {
            try
            {
                if ( attribute != null )
                {
                    var attributeValue = attributeValueService.Queryable().Where( v => v.AttributeId == attribute.Id && v.EntityId == personId ).FirstOrDefault();
                    if ( attributeValue == null )
                    {
                        attributeValue = new AttributeValue();
                        attributeValue.EntityId = personId;
                        attributeValue.AttributeId = attribute.Id;
                        attributeValueService.Add( attributeValue );
                    }
                    attributeValue.Value = value;

                    updateContext.SaveChanges();
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
            public int PersonId { get; set; }
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
