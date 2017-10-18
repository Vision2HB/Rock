using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Rock;
using Rock.Data;
using Rock.Model;

namespace org.willowcreek
{
    public static class Extensions
    {
        #region Badges
        private static readonly string textBadgeDisplayFormat = "<span style='color:{2};font-size:{3}px;font-weight:bold' title='{1}' data-toggle='tooltip'>{0}</span>";
        private static readonly string iconBadgeDisplayFormat = "<img title='{1}' style='vertical-align:text-bottom;height:{2}px;' src='{0}' data-toggle='tooltip'>";

        private static string GetFormattedTextBadge(string badge, string title, string color, int size)
        {
            return string.Format(textBadgeDisplayFormat, badge, title, color, size);
        }

        private static string GetFormattedIconBadge(string badge, string title, int size)
        {
            
            return string.Format(iconBadgeDisplayFormat, badge, title, size);
        }

        /// <summary>
        /// Gets the badge display.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="size">The size.</param>
        /// <param name="httpEncode">if set to <c>true</c> [HTTP encode].</param>
        /// <returns>System.String.</returns>
        public static string GetBadgeDisplay(this Rock.Model.Person person, int size = 38, bool httpEncode = false)
        {
            StringBuilder sb = new StringBuilder();
            ProtectionBadge? protectionBadges = person.GetProtectionBadges();

            if (protectionBadges.HasValue)
            {
                var badge = protectionBadges.Value;
                //attendance
                if (badge.HasFlag(ProtectionBadge.AttendanceRestricted))
                    sb.Append(GetFormattedTextBadge("A", "Restricted", "red", size));
                else if (badge.HasFlag(ProtectionBadge.AttendanceLimited))
                    sb.Append(GetFormattedTextBadge("A", "Limited attendance", "gold", size));

                //protection
                if (badge.HasFlag(ProtectionBadge.ProtectionInitiated))
                    sb.Append(GetFormattedTextBadge("V", "Process Initiated", "blue", size));
                else if (badge.HasFlag(ProtectionBadge.ProtectionInProcess))
                    sb.Append(GetFormattedTextBadge("V", "In process", "purple", size));
                else if (badge.HasFlag(ProtectionBadge.ProtectionNeedsReview))
                    sb.Append(GetFormattedTextBadge("V", "Needs review", "purple", size));
                else if (badge.HasFlag(ProtectionBadge.ProtectionApproved))
                    sb.Append(GetFormattedTextBadge("V", "Cleared to volunteer", "forestgreen", size));
                else if (badge.HasFlag(ProtectionBadge.ProtectionExpired))
                    sb.Append(GetFormattedTextBadge("V", "Inactive or some/all of the Protection requirements have expired", "gray", size));
                else if (badge.HasFlag(ProtectionBadge.ProtectionLimited))
                    sb.Append(GetFormattedTextBadge("V", "Limited volunteering", "gold", size));
                else if (badge.HasFlag(ProtectionBadge.ProtectionRestricted))
                    sb.Append(GetFormattedTextBadge("V", "Restricted from volunteering", "red", size));
                else if (badge.HasFlag(ProtectionBadge.ProtectionUnknown))
                    sb.Append(GetFormattedTextBadge("V", "Unknown Badge State - Contact I.T.", "black", size));

                //youth covenant
                if (badge.HasFlag(ProtectionBadge.YouthInitiated))
                    sb.Append(GetFormattedTextBadge("Y", "Process Initiated", "blue", size));
                else if (badge.HasFlag(ProtectionBadge.YouthInProcess))
                    sb.Append(GetFormattedTextBadge("Y", "In process", "purple", size));
                else if (badge.HasFlag(ProtectionBadge.YouthNeedsReview))
                    sb.Append(GetFormattedTextBadge("Y", "Needs review", "purple", size));
                else if (badge.HasFlag(ProtectionBadge.YouthApproved))
                    sb.Append(GetFormattedTextBadge("Y", "Cleared to volunteer", "forestgreen", size));
                else if (badge.HasFlag(ProtectionBadge.YouthExpired))
                    sb.Append(GetFormattedTextBadge("Y", "Expired due to age or incomplete process", "gray", size));
                else if (badge.HasFlag(ProtectionBadge.YouthIneligible))
                    sb.Append(GetFormattedTextBadge("Y", "Restricted from volunteering", "red", size));
                else if (badge.HasFlag(ProtectionBadge.YouthUnknown))
                    sb.Append(GetFormattedTextBadge("Y", "Unknown Badge State - Contact I.T.", "black", size));

                //membership
                if (badge.HasFlag(ProtectionBadge.MembershipInProcess))
                    sb.Append(GetFormattedTextBadge("M", "Started Membership Process", "purple", size));
                else if (badge.HasFlag(ProtectionBadge.MembershipPendingBaptism))
                    sb.Append(GetFormattedTextBadge("M", "Started Membership Process", "purple", size));
                else if (badge.HasFlag(ProtectionBadge.MembershipPendingUnder18))
                    sb.Append(GetFormattedTextBadge("M", "Started Membership Process", "purple", size));
                else if (badge.HasFlag(ProtectionBadge.MembershipRestricted))
                    sb.Append(GetFormattedTextBadge("M", "Ineligible For Membership", "red", size));
                else if (badge.HasFlag(ProtectionBadge.MembershipCurrent))
                    sb.Append(GetFormattedTextBadge("M", "Is A Member", "green", size));
                else if (badge.HasFlag(ProtectionBadge.MembershipPrevious))
                    sb.Append(GetFormattedTextBadge("M", "Was Previously A Member", "gray", size));
                else if (badge.HasFlag(ProtectionBadge.MembershipUnknown))
                    sb.Append(GetFormattedTextBadge("M", "Unknown Badge State - Contact I.T.", "black", size));

                //leadership
                if (badge.HasFlag(ProtectionBadge.LeadershipApproved))
                    sb.Append(GetFormattedTextBadge("L", "Cleared to lead", "green", size));
                else if (badge.HasFlag(ProtectionBadge.LeadershipExpired))
                    sb.Append(GetFormattedTextBadge("L", "Expired", "gray", size));
                else if (badge.HasFlag(ProtectionBadge.LeadershipInProcess))
                    sb.Append(GetFormattedTextBadge("L", "In process", "purple", size));
                else if (badge.HasFlag(ProtectionBadge.LeadershipLimited))
                    sb.Append(GetFormattedTextBadge("L", "Cleared with restrictions", "gold", size));
                else if (badge.HasFlag(ProtectionBadge.LeadershipIneligible))
                    sb.Append(GetFormattedTextBadge("L", "Unable to Lead", "red", size));
                else if (badge.HasFlag(ProtectionBadge.LeadershipUnknown))
                    sb.Append(GetFormattedTextBadge("L", "Unknown Badge State - Contact I.T.", "black", size));

            }

            //Add Icon Badges
            sb.Append(GetIconBadges(person, size));

            if (httpEncode)
                return System.Web.HttpUtility.HtmlEncode(sb.ToString());
            else
                return sb.ToString();
        }

        /// <summary>
        /// Gets the Protection Badges.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns>System.Nullable&lt;ProtectionBadge&gt;.</returns>
        public static ProtectionBadge? GetProtectionBadges(this Rock.Model.Person person)
        {
            person.LoadAttributes();

            var context = new RockContext();
            var definedValueService = new DefinedValueService(context);
            ProtectionBadge? badge = null;
            var rCampus = person.GetAttributeValue("CampusRestriction");
            var rCareCenter = person.GetAttributeValue("CareCenterRestriction");
            var rGroups = person.GetAttributeValue("GroupsRestriction");
            var rOther = person.GetAttributeValue("OtherRestrictions");
            var rSections = person.GetAttributeValue("SectionsRestriction");
            var rService = person.GetAttributeValue("ServiceRestriction");
            var rCampuses = person.GetAttributeValue("AttendanceRestrictionCampuses");
            var rMembership = person.GetAttributeValue("AttendanceRestrictionMembership");
            var rProtection = person.GetAttributeValue("RestrictedFrom");

            if (rCampus == "All on and off campus events")
            {
                badge = ProtectionBadge.AttendanceRestricted;
            }
            else if (rCampus == "All on and off campus events except weekend services" ||
                     !string.IsNullOrEmpty(rCareCenter) ||
                     !string.IsNullOrEmpty(rGroups) ||
                     !string.IsNullOrEmpty(rOther) ||
                     !string.IsNullOrEmpty(rSections) ||
                     !string.IsNullOrEmpty(rService) ||
                     !string.IsNullOrEmpty(rCampuses))
            {
                badge = ProtectionBadge.AttendanceLimited;
            }
            var protectionStatus = person.GetAttributeValue("ProtectionStatus");
            Guid psGuid;
            if (Guid.TryParse(protectionStatus, out psGuid))
            {
                //If they have restriction set appropriate color, otherwise check normal status
                if(!string.IsNullOrEmpty(rProtection))
                {
                    if(rProtection == "Minors and Vulnerable Adults")
                    {
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.ProtectionLimited;
                        else
                            badge = ProtectionBadge.ProtectionLimited;
                    }
                    else if(rProtection == "Volunteering")
                    {
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.ProtectionRestricted;
                        else
                            badge = ProtectionBadge.ProtectionRestricted;
                    }
                }
                else
                {
                    var ps = definedValueService.GetByGuid(psGuid).Value;
                    switch (ps)
                    {
                        case "Process Initiated":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.ProtectionInitiated;
                            else
                                badge = ProtectionBadge.ProtectionInitiated;
                            break;
                        case "In Progress":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.ProtectionInProcess;
                            else
                                badge = ProtectionBadge.ProtectionInProcess;
                            break;
                        case "Needs Review":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.ProtectionNeedsReview;
                            else
                                badge = ProtectionBadge.ProtectionNeedsReview;
                            break;
                        case "Approved":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.ProtectionApproved;
                            else
                                badge = ProtectionBadge.ProtectionApproved;
                            break;
                        case "Expired":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.ProtectionExpired;
                            else
                                badge = ProtectionBadge.ProtectionExpired;
                            break;
                        case "Approved with restrictions":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.ProtectionLimited;
                            else
                                badge = ProtectionBadge.ProtectionLimited;
                            break;
                        case "Declined":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.ProtectionRestricted;
                            else
                                badge = ProtectionBadge.ProtectionRestricted;
                            break;
                        default:
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.ProtectionUnknown;
                            else
                                badge = ProtectionBadge.ProtectionUnknown;
                            break;
                        case "":
                            break;
                    }
                }
            }

            var youthStatus = person.GetAttributeValue("YouthCovenantStatus");
            Guid ysGuid;
            if (Guid.TryParse(youthStatus, out ysGuid))
            {
                var ys = definedValueService.GetByGuid(ysGuid).Value;
                switch (ys)
                {
                    case "Process Initiated":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.YouthInitiated;
                        else
                            badge = ProtectionBadge.YouthInitiated;
                        break;
                    case "Approved":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.YouthApproved;
                        else
                            badge = ProtectionBadge.YouthApproved;
                        break;
                    case "Expired":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.YouthExpired;
                        else
                            badge = ProtectionBadge.YouthExpired;
                        break;
                    case "In Progress":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.YouthInProcess;
                        else
                            badge = ProtectionBadge.YouthInProcess;
                        break;
                    case "Needs Review":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.YouthNeedsReview;
                        else
                            badge = ProtectionBadge.YouthNeedsReview;
                        break;
                    case "Ineligible":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.YouthIneligible;
                        else
                            badge = ProtectionBadge.YouthIneligible;
                        break;
                    default:
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.YouthUnknown;
                        else
                            badge = ProtectionBadge.YouthUnknown;
                        break;
                    case "":
                        break;
                }
            }

            if (!string.IsNullOrEmpty(rMembership))
                if (badge.HasValue)
                    badge = badge | ProtectionBadge.MembershipRestricted;
                else
                    badge = ProtectionBadge.MembershipRestricted;
            else
            {

                var memberStatus = person.GetAttributeValue("MemberStatus");
                Guid msGuid;
                if (Guid.TryParse(memberStatus, out msGuid))
                {
                    var ms = definedValueService.GetByGuid(msGuid).Value;
                    switch (ms)
                    {
                        case "In Progress":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.MembershipInProcess;
                            else
                                badge = ProtectionBadge.MembershipInProcess;
                            break;
                        case "Pending Baptism":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.MembershipPendingBaptism;
                            else
                                badge = ProtectionBadge.MembershipPendingBaptism;
                            break;
                        case "Pending under 18":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.MembershipPendingUnder18;
                            else
                                badge = ProtectionBadge.MembershipPendingUnder18;
                            break;
                        case "Member":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.MembershipCurrent;
                            else
                                badge = ProtectionBadge.MembershipCurrent;
                            break;
                        case "Previously a member":
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.MembershipPrevious;
                            else
                                badge = ProtectionBadge.MembershipPrevious;
                            break;
                        default:
                            if (badge.HasValue)
                                badge = badge | ProtectionBadge.MembershipUnknown;
                            else
                                badge = ProtectionBadge.MembershipUnknown;
                            break;
                        case "":
                            break;
                    }
                }
            }

            var leadershipStatus = person.GetAttributeValue("LeadershipStatus");
            Guid lsGuid;
            if (Guid.TryParse(leadershipStatus, out lsGuid))
            {
                var ls = definedValueService.GetByGuid(lsGuid).Value;
                switch (ls)
                {
                    case "Cleared to Lead":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.LeadershipApproved;
                        else
                            badge = ProtectionBadge.LeadershipApproved;
                        break;
                    case "Expired":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.LeadershipExpired;
                        else
                            badge = ProtectionBadge.LeadershipExpired;
                        break;
                    case "In Progress":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.LeadershipInProcess;
                        else
                            badge = ProtectionBadge.LeadershipInProcess;
                        break;
                    case "Cleared with restrictions":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.LeadershipLimited;
                        else
                            badge = ProtectionBadge.LeadershipLimited;
                        break;
                    case "Unable to Lead":
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.LeadershipIneligible;
                        else
                            badge = ProtectionBadge.LeadershipIneligible;
                        break;
                    default:
                        if (badge.HasValue)
                            badge = badge | ProtectionBadge.LeadershipUnknown;
                        else
                            badge = ProtectionBadge.LeadershipUnknown;
                        break;
                    case "":
                        break;
                }
            }
            return badge;
        }

        /// <summary>
        /// Gets the Icon Badges
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns>Retruns the icon badges assigned to the person.</returns>
        public static string GetIconBadges(this Rock.Model.Person person, int size)
        {
            StringBuilder sb = new StringBuilder();
            person.LoadAttributes();           
            var rP2CovenantDate = person.GetAttributeValue("P2CovenantDate");
            var rDisabilityDiagnosis = person.GetAttributeValue("DisabilityDiagnosis");
            var r242AffirmationDate = person.GetAttributeValue("242AffirmationDate");

            if (!string.IsNullOrEmpty(r242AffirmationDate))
            {
                sb.Append(GetFormattedIconBadge("/assets/images/242badge.png", "242 Affirmation Received", size));
            }

            if(!string.IsNullOrEmpty(rP2CovenantDate))
            {
                sb.Append(GetFormattedIconBadge("/assets/images/p2badge.png", "P2 Covenant Completed", size));
            }

            if(!string.IsNullOrEmpty(rDisabilityDiagnosis))
            {
                sb.Append(GetFormattedIconBadge("/assets/images/SpecialFriendsBadge.png", "Special Friends", size));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether [is attendance allowed] [the specified badge].
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <returns><c>true</c> if [is attendance allowed] [the specified badge]; otherwise, <c>false</c>.</returns>
        public static bool IsAttendanceAllowed(this ProtectionBadge badge)
        {
            return !badge.HasFlag(ProtectionBadge.AttendanceRestricted);
        }

        /// <summary>
        /// Determines whether the specified badge is restricted.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <returns><c>true</c> if the specified badge is restricted; otherwise, <c>false</c>.</returns>
        public static bool IsRestricted(this ProtectionBadge badge)
        {
            return badge.HasFlag(ProtectionBadge.AttendanceRestricted) ||
                   badge.HasFlag(ProtectionBadge.ProtectionRestricted) ||
                   badge.HasFlag(ProtectionBadge.MembershipRestricted);// || badge.HasFlag(ProtectionBadge.LeadershipRestricted);
        }
        #endregion
    }
}