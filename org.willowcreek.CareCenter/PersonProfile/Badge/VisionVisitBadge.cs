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
    [Description( "Care Center badge to determine the vision status of a person." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Center Vision Badge" )]
    public class VisionVisitBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var attributeKey = "ApprovedForVision";
            var serviceName = "vision";
            var serviceIcon = "icon-eye";
            var sericeAreaGuid = SystemGuid.ServiceArea.SERVICEAREA_VISION;

            var bannedDate = Person.GetBannedDateForServiceArea( sericeAreaGuid );

            var serviceApprovedDate = Person.GetAttributeValue( attributeKey ).AsDateTime();

            var status = "not approved";
            if (serviceApprovedDate.HasValue && serviceApprovedDate >= RockDateTime.Today.AddYears( -1 ) )
            {
                status = "approved";
            } else if ( serviceApprovedDate.HasValue )
            {
                status = "expired";
            }

            var banMarkup = bannedDate.HasValue ? "<div class='banned'></div>" : "";
            var banText = bannedDate.HasValue ? string.Format( "Banned until {0}.", bannedDate.Value.ToShortDateString() ) : "";

            var serviceDescription = string.Format( "{0} is {1} for {2} services{3}. {4}"
                                        , Person.NickName // 0
                                        , status // 1
                                        , serviceName // 2
                                        , status == "approved" ? " through " + serviceApprovedDate.Value.AddYears(1).ToShortDateString() : "" // 3
                                        , banText // 4
                                        );

            writer.Write( String.Format( "<div class='badge badge-servicearea {0} {4}' data-toggle='tooltip' data-original-title='{1}'><i class='badge-icon {2}'></i>{3}</div>"
                    , status.Replace(" ", "-") // 0
                    , serviceDescription   // 1
                    , serviceIcon // 2
                    , banMarkup // 3
                    , serviceName // 4
                    ) );
        }
    }
}