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
    [Description( "Care Center badge to determine to determine if the person is banned." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Center Banned Badge" )]
    public class CareCenterBanBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var sericeAreaGuid = SystemGuid.ServiceArea.SERVICEAREA_CARE_CENTER_GLOBAL;

            var bannedDate = Person.GetBannedDateForServiceArea( sericeAreaGuid );

            if ( bannedDate.HasValue )
            {
                writer.Write( "<span class='label label-danger'>Banned From Care Center</span>" );
            }
        }
    }
}