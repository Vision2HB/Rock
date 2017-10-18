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
using System.Data.Entity;

namespace org.willowcreek.CareCenter.PersonProfile.Badge
{
    /// <summary>
    /// Care Center Visit Count Badge
    /// </summary>
    [Description( "Care Team badge shows how many Care Team visits the person has made." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Team Badge" )]
    public class CareTeamBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var family = Person.GetFamily();

            if ( family != null )
            {
                var rockContext = new RockContext();

                var careTeamVisitCount = new AssessmentService( rockContext )
                    .Queryable().AsNoTracking()
                    .ForPerson( Person.Id )
                    .WithCompletedWorkflow()
                    .Count();

                var message = careTeamVisitCount > 0 ?
                    string.Format( "{0} has visited the Care Team {1} times.", Person.NickName, careTeamVisitCount ) :
                    string.Format( "{0} has not visited the Care Team yet.", Person.NickName );

                writer.Write( String.Format( "<div class='badge badge-careteamcount {0}' data-toggle='tooltip' data-original-title='{1}'><div class='badge-details'><span>CT</span></div></div>", careTeamVisitCount > 0 ? "has-visited" : "hasnot-visited", message ) );
            }
        }
    }
}
