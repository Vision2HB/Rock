using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Visit Service
    /// </summary>
    /// <seealso cref="Rock.Data.Service{org.willowcreek.CareCenter.Model.Visit}" />
    public partial class VisitService
    {

        /// <summary>
        /// Gets the the first visit for selected person today.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public Visit GetCurrentVisit( int personId )
        {
            var startTime = RockDateTime.Now.AddHours( -4 );

            return this.Queryable()
                .Where( v =>
                    v.PersonAlias.PersonId == personId &&
                    v.VisitDate >= startTime )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the last completed visits of a particular type for the specified family.
        /// </summary>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="serviceAreaGuid">The service area unique identifier.</param>
        /// <returns></returns>
        public IQueryable<Visit> GetLastVisits( int familyId, string serviceAreaGuid )
        {
            Guid guid = serviceAreaGuid.AsGuid();

            int workflowTypeId = 0;

            var rockContext = (RockContext)this.Context;
            var serviceArea = new ServiceAreaService( rockContext ).Get( guid );
            if ( serviceArea != null )
            {
                var familyPersonIds = new GroupMemberService( rockContext ).GetByGroupId( familyId )
                    .AsNoTracking()
                    .Select( m => m.PersonId )
                    .ToList();

                var qry = Queryable().Where( v =>
                    v.PersonAlias != null &&
                    familyPersonIds.Contains( v.PersonAlias.PersonId ) );

                if ( serviceArea.WorkflowTypeId.HasValue )
                {
                    workflowTypeId = serviceArea.WorkflowTypeId.Value;

                    qry = qry.Where( v =>
                        v.Workflows.Any( w =>
                            w.WorkflowTypeId == workflowTypeId &&
                            w.Status == Constant.WORKFLOW_STATUS_COMPLETED ) );
                }

                return qry.OrderByDescending( v => v.VisitDate );
            }

            return null;
        }


        /// <summary>
        /// Updates the workflow passport status.
        /// </summary>
        /// <param name="visitId">The visit identifier.</param>
        public void UpdateWorkflowPassportStatus( int visitId )
        {
            var visit = this.Get( visitId );
            if ( visit != null )
            {
                var rockContext = (RockContext)this.Context;

                foreach ( var workflow in visit.Workflows )
                {
                    string visitStatus = visit.PassportStatus.ConvertToInt().ToString();
                    workflow.LoadAttributes();
                    if ( workflow.Attributes.ContainsKey( "PassportStatus" ) && workflow.GetAttributeValue( "PassportStatus" ) != visitStatus )
                    {
                        workflow.SetAttributeValue( "PassportStatus", visitStatus );
                        workflow.SaveAttributeValues( rockContext );

                        List<string> workflowErrors;
                        new Rock.Model.WorkflowService( rockContext ).Process( workflow, out workflowErrors );
                        if ( !workflowErrors.Any() )
                        {
                            rockContext.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether [is first visit] [the specified visit identifier].
        /// </summary>
        /// <param name="visitId">The visit identifier.</param>
        /// <returns></returns>
        public bool IsFirstVisit( int visitId )
        {
            var visit = this.Get( visitId );
            if ( visit != null )
            {
                if ( Queryable().AsNoTracking()
                    .Where( v =>
                        v.Id != visit.Id &&
                        v.PersonAlias != null &&
                        v.PersonAlias.PersonId == visit.PersonAlias.PersonId &&
                        v.VisitDate < visit.VisitDate )
                    .Any() )
                {
                    return false;
                }
            }
            return true;
        }

    }

    /// <summary>
    /// Visit Extension Methods
    /// </summary>
    public static partial class VisitExtensionMethods
    {

        /// <summary>
        /// Returns boolean indication if this is the person's first visit.
        /// </summary>
        /// <param name="visit">The visit.</param>
        /// <returns></returns>
        public static bool FirstVisit( this Visit visit )
        {
            if ( visit != null )
            {
                return new VisitService( new RockContext() ).IsFirstVisit( visit.Id );
            }
            return true;
        }
    }
}
