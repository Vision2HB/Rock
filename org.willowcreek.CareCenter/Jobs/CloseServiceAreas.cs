using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using org.willowcreek.CareCenter.Model;
using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace org.willowcreek.CareCenter.Jobs
{
    /// <summary>
    /// Job to close workflows
    /// </summary>
    [CustomCheckboxListField( "Workflows", "The workflow types to cancel if not yet completed", @"
        SELECT 
	        CAST(W.[Guid] AS VARCHAR(50)) AS [Value], 
	        C.[Name] + ': ' + W.[Name] AS [Text] 
        FROM [WorkflowType] W
        INNER JOIN [Category] C ON C.[Id] = W.[CategoryId]
        WHERE C.[Guid] IN ( '11601938-4335-41FA-9D44-04CA1054649E', 'C068535D-F1AD-430A-9F65-57E228A3CF86' )
        ORDER BY C.[Name], W.[Name]
", true, "", "", 0)]

    [TextField("Workflow Status", "The status to set the workflows to when marking them complete.", true, "Complete", order: 1)]
    [EnumField("Visit Status", "The status that the visits should be set to when marking them complete.", typeof(VisitStatus), true, order: 2)]

    [DisallowConcurrentExecution]
    public class CloseServiceAreas : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CloseServiceAreas()
        {
        }

        /// <summary>
        /// Job that will close workflows.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var workflowTypeGuids = dataMap.GetString( "Workflows" ).SplitDelimitedValues().ToList().AsGuidList();

            var workflowStatus = dataMap.GetString( "WorkflowStatus" );
            var visitStatus = (VisitStatus)dataMap.GetIntValue( "VisitStatus" );

            using ( RockContext rockContext = new RockContext() )
            {
                var workflowIds = new List<int>();

                var today = RockDateTime.Today;
                var appointments = new WorkflowAppointmentService( rockContext )
                    .Queryable()
                    .Where( a => a.AppointmentDate >= today );

                // Find any non-completed workflows of the selected type that are not associated with an appointment and 'cancel' them.
                foreach( var workflow in new WorkflowService( rockContext )
                    .Queryable()
                    .Where( w =>
                        workflowTypeGuids.Contains( w.WorkflowType.Guid ) &&
                        w.CompletedDateTime == null &&
                        !appointments.Any( a => a.WorkflowId == w.Id ) )
                    .ToList() )
                {
                    workflowIds.Add( workflow.Id );

                    workflow.AddLogEntry( "Service Area Cancelled" );
                    workflow.CompletedDateTime = RockDateTime.Now;
                    workflow.Status = workflowStatus;

                    rockContext.SaveChanges();
                }

                // Find any visits associated with one or more of the workflows just cancelled and check to see if it should be completed/cancelled
                foreach ( var visit in new VisitService( rockContext ).Queryable()   
                    .Where( v =>
                        v.Workflows.Any( w => workflowIds.Contains( w.Id ) ) &&
                        !v.Workflows.Any( w => w.CompletedDateTime == null ) )
                    .ToList() )
                {
                    visit.Status = visitStatus;
                    rockContext.SaveChanges();
                }

            }

        }

    }
}
