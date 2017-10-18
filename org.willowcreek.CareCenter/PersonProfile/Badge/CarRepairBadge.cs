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
    /// Care Center Vision Badge
    /// </summary>
    [Description( "Care Center badge to determine the car repair status of a person." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Center Car Repair Badge" )]
    public class CarRepairBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var autoRepairStatus = Person.GetAutoRepairStatus();

            var status = "not approved";
            if ( autoRepairStatus.ApprovalDate.HasValue )
            {
                if ( autoRepairStatus.ApprovalDate.Value >= RockDateTime.Today.AddYears( -1 ) )
                {
                    status = "approved";
                }
                else
                {
                    status = "expired";
                }
            }

            string tooMany = string.Empty;
            var statusCss = status;
            if ( status == "approved" )
            {
                bool allowed = autoRepairStatus.AttendsWillow ? autoRepairStatus.AutoRepairsLastYear < 2 : autoRepairStatus.AutoRepairsLastYear < 1;
                if ( !allowed )
                {
                    statusCss = "expired";
                    tooMany = string.Format( ", however this family has alreay had {0} {1} in the last year", autoRepairStatus.AutoRepairsLastYear,
                        "repair".PluralizeIf( autoRepairStatus.AutoRepairsLastYear > 1 ) );
                }
            }

            string banMarkup = string.Empty;
            string banText = string.Empty;
            if ( autoRepairStatus.BanExpireDate.HasValue && autoRepairStatus.BanExpireDate.Value >= RockDateTime.Today )
            {
                banMarkup = "<div class='banned'></div>";
                banText = string.Format( "Banned until {0}", autoRepairStatus.BanExpireDate.Value.ToShortDateString() );
            }

            var serviceDescription = string.Format( "{0} is {1} for Auto Repair services{2}{3}. {4}"
                , Person.NickName   // 0
                , status            // 1
                , status == "approved" ? " through " + autoRepairStatus.ApprovalDate.Value.AddYears( 1 ).ToShortDateString() : "" // 2
                , tooMany           // 3
                , banText           // 4
            );

            writer.Write( String.Format( "<div class='badge badge-servicearea {0} car-repair' data-toggle='tooltip' data-original-title='{1}'><i class='fa fa-car fa-3x'></i>{2}</div>"
                    , statusCss.Replace( " ", "-" ) // 0
                    , serviceDescription            // 1
                    , banMarkup                     // 2
                    ) );
        }
    }
}