﻿// <copyright>
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
using System.Linq;
using System.Text;
using System.Web;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace com.bemaservices.ReportingTools.Jobs
{
    /// <summary>
    /// Job to run quick lava code on a schedule
    /// </summary>
    [IntegerField( "Days Back to Sync Family Names For", "Limit the sync to only families updated within the last X days. Leave blank to sync all families", false, Key = "DaysToSyncFamilyNames" )]
    [DisallowConcurrentExecution]
    public class UpdateFamilyNames : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdateFamilyNames()
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
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // run lava code to do something
            string lavaCode = dataMap.GetString( "Value" );
            string lavaCommands = dataMap.GetString( "EnabledLavaCommands" );
            try
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                string value = lavaCode.ResolveMergeFields( mergeFields, lavaCommands ).Trim();

                context.Result = value;
            }
            catch ( System.Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw;
            }

        }

    }
}
