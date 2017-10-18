using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;

using org.willowcreek;

using Rock;
using Rock.Model;
using Rock.PersonProfile;
using Rock.Web.Cache;

namespace org.willowcreek.CareCenter.PersonProfile.Badge
{
    /// <summary>
    /// Care Center Vision Badge
    /// </summary>
    [Description( "Care Center Protection Attendance Badge." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Care Center Protection Badge" )]
    public class CareCenterProtectionBadge : BadgeComponent
    {

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            StringBuilder sb = new StringBuilder();

            var campusBans = Person.GetAttributeValue( "AttendanceRestrictionCampuses" ).SplitDelimitedValues().AsGuidList();

            var sericeAreaGuid = SystemGuid.ServiceArea.SERVICEAREA_CARE_CENTER_GLOBAL;
            var bannedDate = Person.GetBannedDateForServiceArea( sericeAreaGuid );
            if ( bannedDate.HasValue )
            {
                sb.Append( GetFormattedTextBadge( "Banned from Care Center", "danger" ) );
            }
            else
            {
                ProtectionBadge? protectionBadges = Person.GetProtectionBadges();

                if ( protectionBadges.HasValue )
                {
                    var protectionBadge = protectionBadges.Value;

                    if ( protectionBadge.HasFlag( ProtectionBadge.AttendanceRestricted ) )
                    {
                        sb.Append( GetFormattedTextBadge( "Restricted Attendance", "danger" ) );
                    }
                    else if ( campusBans.Contains( new Guid( "76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8" ) ) )  // Banned from South Barrington Campus
                    {
                        sb.Append( GetFormattedTextBadge( "Campus Restriction", "danger" ) );
                    }
                    else if ( !string.IsNullOrEmpty( Person.GetAttributeValue( "CareCenterRestriction" ) ) )
                    {
                        sb.Append( GetFormattedTextBadge( "Care Center Restricted", "danger" ) );
                    }
                }
            }

            writer.Write( sb.ToString() );
        }

        private string GetFormattedTextBadge( string description, string cssClass )
        {
            return string.Format( "<div class='badge' data-toggle='tooltip' data-original-title='{1}'><div class='badge-details'><span class='text-{0}' style='font-size:38px;font-weight:bold'>A</span></div></div>", cssClass, description );
        }
    }
}