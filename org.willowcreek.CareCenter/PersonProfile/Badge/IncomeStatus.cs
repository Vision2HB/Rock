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

namespace org.willowcreek.CareCenter.PersonProfile.Badge
{
    /// <summary>
    /// Care Center Income Status Badge
    /// </summary>
    [Description( "Care Center badge that if someone meets the income status levels." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Center Income Status Badge" )]
    
    public class IncomeStatusBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var incomeStatus = Person.GetIncomeStatus();

            switch ( incomeStatus )
            {
                case CareCenter.IncomeStatus.Meets:
                    {
                        writer.Write( String.Format( "<div class='badge badge-incomestatus meets' data-toggle='tooltip' data-original-title='{0} meets the income requirements.'></div>", Person.NickName ) );
                        break;
                    }
                case CareCenter.IncomeStatus.DoesNotMeet:
                    {
                        writer.Write( String.Format( "<div class='badge badge-incomestatus not-meet' data-toggle='tooltip' data-original-title='{0} does not meet the income requirements.'></div>", Person.NickName ) );
                        break;
                    }
                case CareCenter.IncomeStatus.Unknown:
                    {
                        writer.Write( String.Format( "<div class='badge badge-incomestatus unknown' data-toggle='tooltip' data-original-title='Could not determine if {0} meets the income requirements.'></div>", Person.NickName ) );
                        break;
                    }
            }
        }
    }
}
