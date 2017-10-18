using System;
using System.Linq;

namespace org.willowcreek
{
    [Flags]
    public enum ProtectionBadge
    {
        //Note - These are bitwise representations not actual integers
        AttendanceLimited = 1,
        AttendanceRestricted = 2,

        ProtectionInitiated = 4194304,
        ProtectionInProcess = 4,
        ProtectionNeedsReview = 8,
        ProtectionApproved = 16,
        ProtectionLimited = 32,
        ProtectionExpired = 64,
        ProtectionRestricted = 128,
        ProtectionUnknown = 256,

        MembershipInProcess = 512,
        MembershipPendingBaptism = 1024,
        MembershipPendingUnder18 = 2048,
        MembershipCurrent = 4096,
        MembershipPrevious = 8192,
        MembershipUnknown = 16384,
        MembershipRestricted = 32768,

        LeadershipApproved = 65536,
        LeadershipExpired = 131072,
        LeadershipInProcess = 262144,
        LeadershipLimited = 524288,
        LeadershipIneligible = 1048576,
        LeadershipUnknown = 2097152,

        YouthInitiated = 8388608,
        YouthInProcess = 16777216,
        YouthNeedsReview = 33554432,
        YouthApproved = 67108864,
        YouthIneligible = 134217728,
        YouthExpired = 268435456,
        YouthUnknown = 536870912
    }
}
