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
    /// Assessment Service
    /// </summary>
    /// <seealso cref="Rock.Data.Service{org.willowcreek.CareCenter.Model.Assessment}" />
    public partial class AssessmentService
    {

        /// <summary>
        /// Determines whether [is first assessment] [the specified assessment identifier].
        /// </summary>
        /// <param name="assessmentId">The assessment identifier.</param>
        /// <returns></returns>
        public bool IsFirstAssessment( int assessmentId )
        {
            var assessment = this.Get( assessmentId );
            if ( assessment != null )
            {
                if ( Queryable().AsNoTracking()
                    .Where( v =>
                        v.Id != assessment.Id &&
                        v.PersonAlias != null &&
                        v.PersonAlias.PersonId == assessment.PersonAlias.PersonId &&
                        v.AssessmentDate < assessment.AssessmentDate )
                    .Any() )
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Assessment Extension Methods
    /// </summary>
    public static partial class AssessmentExtensionMethods
    {
        /// <summary>
        /// Returns boolean indication if this is the person's first assessment.
        /// </summary>
        /// <param name="assessment">The assessment.</param>
        /// <returns></returns>
        public static bool FirstAssessment( this Assessment assessment )
        {
            if ( assessment != null )
            {
                return new AssessmentService( new RockContext() ).IsFirstAssessment( assessment.Id );
            }
            return true;
        }

        /// <summary>
        /// Gets assessments for a specific person.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public static IQueryable<Assessment> ForPerson( this IQueryable<Assessment> qry, int personId )
        {
            return qry.Where( a => a.PersonAlias != null && a.PersonAlias.PersonId == personId );
        }


        /// <summary>
        /// Gets assessments with completed workflows.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <returns></returns>
        public static IQueryable<Assessment> WithCompletedWorkflow( this IQueryable<Assessment> qry )
        {
            return qry.Where( a => a.Workflows.Any( w => w.CompletedDateTime != null ) );
        }
    }
}
