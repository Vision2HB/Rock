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
    /// Care Center Clothing Visit Badge
    /// </summary>
    [Description( "Care Center badge to determine the clothing visit status of a person." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Center Clothing Visit Badge" )]

    public class ClothingVisitBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var clothingVisitStatus = Person.GetClothingVisitStatus();

            var banMarkup = clothingVisitStatus.IsBanned ? "<div class='banned'></div>" : "";
            var banText = clothingVisitStatus.IsBanned ? string.Format( "Banned until {0}.", clothingVisitStatus.BanExpireDate.Value.ToShortDateString() ) : "";
            var clothingClass = clothingVisitStatus.ClothingLevel.ToString().ToLower();
            var clothingDescription = clothingVisitStatus.ClothingLevel == ClothingLevel.Full ? " is allowed full visits." : " is allowed limited visits.";

            writer.Write( String.Format( "<div class='badge badge-clothingvisit {2}' data-toggle='tooltip' data-original-title='{4} {3} {0}'><i class='badge-icon icon-hanger'></i>{1}</div>"
                    , banText   // 0
                    , banMarkup // 1
                    , clothingClass // 2
                    , clothingDescription // 3
                    , Person.NickName // 4
                    ) );
        }
    }
}