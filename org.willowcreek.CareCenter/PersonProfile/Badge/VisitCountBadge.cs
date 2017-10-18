using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;
using System.Diagnostics;
using Rock.Web.Cache;
using Humanizer;
using Rock.PersonProfile;
using Rock;
using org.willowcreek.CareCenter;
using org.willowcreek.CareCenter.Model;

namespace org.willowcreek.CareCenter.PersonProfile.Badge
{
    /// <summary>
    /// Care Center Visit Count Badge
    /// </summary>
    [Description( "Care Center badge shows how many visits the person has made." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Center Visit Count Badge" )]
    
    public class VisitCountBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var family = Person.GetFamily();

            if ( Person != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var familyPersonIds = Person.GetFamilies( rockContext )
                        .SelectMany( f => f.Members )
                        .Select( m => m.PersonId )
                        .Distinct()
                        .ToList();

                    var visitService = new VisitService( rockContext );

                    // get number of visits in the last 12 monthes
                    var tweleveMonthDate = RockDateTime.Now.AddDays( -365 );
                    var visitsInTwelveMonths = visitService.Queryable()
                                                    .Where( v =>
                                                        v.PersonAlias != null &&
                                                        familyPersonIds.Contains(v.PersonAlias.PersonId) && 
                                                        v.VisitDate > tweleveMonthDate && 
                                                        v.Status == VisitStatus.Complete ) 
                                                    .Count();

                    // get total number of visits
                    var totalVisits = visitService.Queryable()
                                                   .Where( v =>
                                                        v.PersonAlias != null &&
                                                        familyPersonIds.Contains( v.PersonAlias.PersonId ) &&
                                                        v.Status == VisitStatus.Complete )
                                                   .Count();

                    writer.Write( String.Format( "<div class='badge badge-visitcount' data-toggle='tooltip' data-original-title='{0} has visited {1} times in the last 12 months and {2} times in total.'><div class='visit-metric'><div class='visits-year'>{1}</div><div class='visits-total'>/{2}</div></div></div>", Person.NickName, visitsInTwelveMonths, totalVisits ) );

                }
            }
        }
    }
}
