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
    /// Care Center Food Visit Badge
    /// </summary>
    [Description( "Care Center badge to determine the food visit status of a person." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Center Food Visit Badge" )]

    public class FoodVisitBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            var foodVisitStatus = Person.GetFoodVisitStatus();

            var badgeColor = "food";
            var badgeText = " is eligible for a food visit. ";

            if ( !foodVisitStatus.FoodVisitAllowed )
            {
                if ( foodVisitStatus.BreadVisitAllowed )
                {
                    badgeColor = "bread";
                    badgeText = " is eligible for a bread visit.";
                }
                else
                {
                    badgeColor = "none";
                    badgeText = " is not eligible for a visit.";
                }
            }

            var visitsPerMonth = foodVisitStatus.FoodVisitsAllowedPerMonth;
            var visitTerm = "visit";

            badgeText += $" Is allowed {visitTerm.ToQuantity(visitsPerMonth)} per month.";

            if ( foodVisitStatus.GraceVisitAllowed )
            {
                badgeText += " Has a grace visit (G).";
            }

            var foodcountClass = "foodcount-" + foodVisitStatus.FoodVisitsAllowedPerMonth;

            var graceClass = foodVisitStatus.GraceVisitAllowed ? "grace-allowed" : "grace-notallowed";
            var courtesyClass = foodVisitStatus.CourtesyVisitAllowed ? "courtesy-allowed" : "courtesy-notallowed";

            var banMarkup = foodVisitStatus.IsBanned ? "<div class='banned'></div>" : "";
            var banText = foodVisitStatus.IsBanned ? string.Format( "Banned until {0}.", foodVisitStatus.BanExpireDate.Value.ToShortDateString() ) : "";         

            writer.Write( String.Format( "<div class='badge badge-foodvisits {0} {1} {2} {3}' data-toggle='tooltip' data-original-title='{5} {6} {7}'><i class='badge-icon icon-apple'></i> <div class='details'><span class='foodcount'>{8}</span><span>&nbsp;</span><span class='gracevisit'>G</span></div>{4}</div>"
                , badgeColor        // 0
                , graceClass        // 1
                , courtesyClass     // 2
                , foodcountClass    // 3
                , banMarkup         // 4
                , Person.NickName   // 5
                , badgeText         // 6
                , banText           // 7
                , visitsPerMonth    // 8
                ) );
        }
    }
}