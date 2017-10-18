using System;
using System.Collections.Generic;
using System.Linq;
using org.willowcreek.CareCenter.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.willowcreek.CareCenter
{
    /// <summary>
    /// Boolean Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Person Extensions

        /// <summary>
        /// Gets the income status.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static IncomeStatus GetIncomeStatus( this Person person )
        {
            // get family income
            var family = person.GetFamily();

            if ( family != null )
            {
                return family.GetIncomeStatus();
            }
            else
            {
                return IncomeStatus.Unknown;
            }
        }

        public static IncomeStatus GetIncomeStatus( this Person person, decimal? familyIncome, bool familyAttendsWillow )
        {
            // get family income
            var family = person.GetFamily();

            if ( family != null )
            {
                return family.GetIncomeStatus( familyIncome, familyAttendsWillow );
            }
            else
            {
                return IncomeStatus.Unknown;
            }
        }

        /// <summary>
        /// Gets the food visit status.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static FoodVisitStatus GetFoodVisitStatus( this Person person )
        {
            // get family food status
            var family = person.GetFamily();
            var foodStatus = family != null ? family.GetFoodVisitStatus() : new FoodVisitStatus();

            // get banned info
            var banService = new ServiceAreaBanService( new RockContext() );

            var foodExpirationDate = banService.GetBanExpirationDate( person.Id, SystemGuid.ServiceArea.SERVICEAREA_FOOD );
            if ( foodExpirationDate.HasValue )
            {
                foodStatus.IsBanned = true;
                foodStatus.BanExpireDate = foodExpirationDate.Value;
            }

            var breadExpirationDate = banService.GetBanExpirationDate( person.Id, SystemGuid.ServiceArea.SERVICEAREA_BREAD );
            if ( breadExpirationDate.HasValue )
            {
                foodStatus.IsBanned = true;
                if ( !foodExpirationDate.HasValue || foodExpirationDate < breadExpirationDate.Value )
                {
                    foodStatus.BanExpireDate = breadExpirationDate.Value;
                }
            }

            return foodStatus;
        }

        /// <summary>
        /// Gets the clothing visit status.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static ClothingVisitStatus GetClothingVisitStatus( this Person person )
        {
            // get family clothing status
            var family = person.GetFamily();
            var clothingStatus = family != null ? family.GetClothingVisitStatus() : new ClothingVisitStatus();

            // get banned info
            var banService = new ServiceAreaBanService( new RockContext() );

            var clothingExpirationDate = banService.GetBanExpirationDate( person.Id, SystemGuid.ServiceArea.SERVICEAREA_CLOTHING );
            if ( clothingExpirationDate.HasValue )
            {
                clothingStatus.IsBanned = true;
                clothingStatus.BanExpireDate = clothingExpirationDate.Value;
            }

            var limitedClothingExpirationDate = banService.GetBanExpirationDate( person.Id, SystemGuid.ServiceArea.SERVICEAREA_LIMITED_CLOTHING );
            if ( limitedClothingExpirationDate.HasValue )
            {
                clothingStatus.IsBanned = true;
                if ( !clothingExpirationDate.HasValue || clothingExpirationDate < limitedClothingExpirationDate.Value )
                {
                    clothingStatus.BanExpireDate = limitedClothingExpirationDate.Value;
                }
            }

            return clothingStatus;
        }

        public static AutoRepairStatus GetAutoRepairStatus( this Person person )
        {
            if ( person.Attributes == null )
            {
                person.LoadAttributes();
            }

            AutoRepairStatus autoRepairStatus = null;

            // get family status
            var family = person.GetFamily();
            if ( family != null )
            {
                autoRepairStatus = family.GetAutoRepairStatus();
            }
            else
            {
                autoRepairStatus = new AutoRepairStatus();
                autoRepairStatus.AttendsWillow = false;
                autoRepairStatus.AutoRepairDates = new List<DateTime>();

                var mostRecentAutoRepairAttr = AttributeCache.Read( SystemGuid.Attribute.PERSON_MOST_RECENT_AUTO_REPAIR.AsGuid() );
                var previousAutoRepairAttr = AttributeCache.Read( SystemGuid.Attribute.PERSON_PREVIOUS_AUTO_REPAIR.AsGuid() );

                DateTime? mostRecentAutoRepair = person.GetAttributeValue( mostRecentAutoRepairAttr.Key ).AsDateTime();
                if ( mostRecentAutoRepair.HasValue )
                {
                    autoRepairStatus.AutoRepairDates.Add( mostRecentAutoRepair.Value );
                }

                DateTime? previousAutoRepair = person.GetAttributeValue( previousAutoRepairAttr.Key ).AsDateTime();
                if ( previousAutoRepair.HasValue )
                {
                    autoRepairStatus.AutoRepairDates.Add( previousAutoRepair.Value );
                }
            }

            var lastYear = RockDateTime.Today.AddYears( -1 );
            autoRepairStatus.AutoRepairsLastYear = autoRepairStatus.AutoRepairDates.Where( d => d > lastYear ).Count();

            var approvedAttr = AttributeCache.Read( SystemGuid.Attribute.PERSON_APPROVED_FOR_CAR_REPAIR.AsGuid() );
            autoRepairStatus.ApprovalDate = person.GetAttributeValue( approvedAttr.Key ).AsDateTime();

            // get banned info
            var banService = new ServiceAreaBanService( new RockContext() );

            var autoRepairExpirationDate = banService.GetBanExpirationDate( person.Id, SystemGuid.ServiceArea.SERVICEAREA_AUTO_REPAIR );
            if ( autoRepairExpirationDate.HasValue )
            {
                autoRepairStatus.IsBanned = true;
                autoRepairStatus.BanExpireDate = autoRepairExpirationDate.Value;
            }

            bool approvalStillGood = autoRepairStatus.ApprovalDate.HasValue && autoRepairStatus.ApprovalDate.Value > RockDateTime.Today.AddYears( -1 );
            bool repairRemaining = autoRepairStatus.AttendsWillow ? autoRepairStatus.AutoRepairsLastYear < 2 : autoRepairStatus.AutoRepairsLastYear < 1;
            bool notBanned = !autoRepairStatus.BanExpireDate.HasValue || autoRepairStatus.BanExpireDate.Value > RockDateTime.Today;
            autoRepairStatus.AutoRepairAllowed = approvalStillGood && repairRemaining && notBanned;

            return autoRepairStatus;
        }

        /// <summary>
        /// Gets the resource visit status.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static ResourceVisitStatus GetResourceVisitStatus( this Person person )
        {
            // get family clothing status
            var family = person.GetFamily();
            var resourceStatus = family != null ? family.GetResourceVisitStatus() : new ResourceVisitStatus();

            // get banned info
            var banService = new ServiceAreaBanService( new RockContext() );

            var resourceExpirationDate = banService.GetBanExpirationDate( person.Id, SystemGuid.ServiceArea.SERVICEAREA_RESOURCE );
            if ( resourceExpirationDate.HasValue )
            {
                resourceStatus.IsBanned = true;
                resourceStatus.BanExpireDate = resourceExpirationDate.Value;
            }

            return resourceStatus;
        }


        /// <summary>
        /// Gets the banned date for service area.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="serviceAreaId">The service area identifier.</param>
        /// <returns></returns>
        public static DateTime? GetBannedDateForServiceArea( this Person person, int serviceAreaId )
        {
            return new ServiceAreaBanService( new RockContext() ).GetBanExpirationDate( person.Id, serviceAreaId );
        }

        public static DateTime? GetBannedDateForServiceArea( this Person person, Guid serviceAreaGuid )
        {
            return new ServiceAreaBanService( new RockContext() ).GetBanExpirationDate( person.Id, serviceAreaGuid );
        }

        /// <summary>
        /// Gets the banned date for service area.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="serviceAreaGuid">The service area unique identifier.</param>
        /// <returns></returns>
        public static DateTime? GetBannedDateForServiceArea( this Person person, string serviceAreaGuid )
        {
            return new ServiceAreaBanService( new RockContext() ).GetBanExpirationDate( person.Id, serviceAreaGuid );
        }

        #endregion
    }
    
    #region Enums
    /// <summary>
    /// Income Status (compared to poverty level) enum
    /// </summary>
    public enum IncomeStatus
    {
        DoesNotMeet = 0,
        Meets = 1,
        Unknown = 2
    }

    /// <summary>
    /// List of Clothing Levels
    /// </summary>
    public enum ClothingLevel
    {
        Full = 0,
        Limited = 1
    }
    #endregion

    #region Result Classes

    /// <summary>
    /// Food Visit Status Result
    /// </summary>
    public class FoodVisitStatus {
        /// <summary>
        /// Gets or sets a value indicating whether [food visit allowed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [food visit allowed]; otherwise, <c>false</c>.
        /// </value>
        public bool FoodVisitAllowed { get; set; }

        /// <summary>
        /// Gets or sets the last food visit.
        /// </summary>
        /// <value>
        /// The last food visit.
        /// </value>
        public DateTime? LastFoodVisit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [bread visit allowed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bread visit allowed]; otherwise, <c>false</c>.
        /// </value>
        public bool BreadVisitAllowed { get; set; }

        /// <summary>
        /// Gets or sets the last bread visit.
        /// </summary>
        /// <value>
        /// The last bread visit.
        /// </value>
        public DateTime? LastBreadVisit { get; set; }

        /// <summary>
        /// Gets or sets the food visits allowed per month.
        /// </summary>
        /// <value>
        /// The food visits allowed per month.
        /// </value>
        public int FoodVisitsAllowedPerMonth { get; set; }

        /// <summary>
        /// Gets or sets the food visit override expire date.
        /// </summary>
        /// <value>
        /// The food visit override expire date.
        /// </value>
        public DateTime? FoodVisitOverrideExpireDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [grace visit allowed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [grace visit allowed]; otherwise, <c>false</c>.
        /// </value>
        public bool GraceVisitAllowed { get; set; }

        /// <summary>
        /// Gets or sets the last grace visit date.
        /// </summary>
        /// <value>
        /// The last grace visit date.
        /// </value>
        public DateTime? LastGraceVisitDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [courtesy visit allowed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [courtesy visit allowed]; otherwise, <c>false</c>.
        /// </value>
        public bool CourtesyVisitAllowed { get; set; }

        /// <summary>
        /// Gets or sets the last courtesy visit date.
        /// </summary>
        /// <value>
        /// The last courtesy visit date.
        /// </value>
        public DateTime? LastCourtesyVisitDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is banned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is banned; otherwise, <c>false</c>.
        /// </value>
        public bool IsBanned { get; set; }

        /// <summary>
        /// Gets or sets the ban expire date.
        /// </summary>
        /// <value>
        /// The ban expire date.
        /// </value>
        public DateTime? BanExpireDate { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ClothingVisitStatus
    {
        /// <summary>
        /// Gets or sets the clothing level.
        /// </summary>
        /// <value>
        /// The clothing level.
        /// </value>
        public ClothingLevel ClothingLevel { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this instance is banned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is banned; otherwise, <c>false</c>.
        /// </value>
        public bool IsBanned { get; set; }

        /// <summary>
        /// Gets or sets the ban expire date.
        /// </summary>
        /// <value>
        /// The ban expire date.
        /// </value>
        public DateTime? BanExpireDate { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AutoRepairStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether [automatic repair allowed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [automatic repair allowed]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRepairAllowed { get; set; }

        /// <summary>
        /// Gets or sets the approval date.
        /// </summary>
        /// <value>
        /// The approval date.
        /// </value>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Gets or sets the automatic repairs last yer.
        /// </summary>
        /// <value>
        /// The automatic repairs last yer.
        /// </value>
        public int AutoRepairsLastYear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [attends willow].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attends willow]; otherwise, <c>false</c>.
        /// </value>
        public bool AttendsWillow { get; set; }

        /// <summary>
        /// Gets or sets the automatic repair dates.
        /// </summary>
        /// <value>
        /// The automatic repair dates.
        /// </value>
        public List<DateTime> AutoRepairDates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is banned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is banned; otherwise, <c>false</c>.
        /// </value>
        public bool IsBanned { get; set; }

        /// <summary>
        /// Gets or sets the ban expire date.
        /// </summary>
        /// <value>
        /// The ban expire date.
        /// </value>
        public DateTime? BanExpireDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [receive automatic allowed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [receive automatic allowed]; otherwise, <c>false</c>.
        /// </value>
        public bool ReceiveAutoAllowed { get; set; }

        /// <summary>
        /// Gets or sets the received automatic date.
        /// </summary>
        /// <value>
        /// The received automatic date.
        /// </value>
        public DateTime? ReceivedAutoDate { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResourceVisitStatus
    {
        /// <summary>
        /// Gets or sets the clothing level.
        /// </summary>
        /// <value>
        /// The clothing level.
        /// </value>
        public bool ResourceVisitAllowed { get; set; }

        /// <summary>
        /// Gets or sets the last resource visit.
        /// </summary>
        /// <value>
        /// The last resource visit.
        /// </value>
        public DateTime? LastResourceVisit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [food visit threshold].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [food visit threshold]; otherwise, <c>false</c>.
        /// </value>
        public int? FoodVisitThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [food visits].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [food visits]; otherwise, <c>false</c>.
        /// </value>
        public int FoodVisits { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is banned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is banned; otherwise, <c>false</c>.
        /// </value>
        public bool IsBanned { get; set; }

        /// <summary>
        /// Gets or sets the ban expire date.
        /// </summary>
        /// <value>
        /// The ban expire date.
        /// </value>
        public DateTime? BanExpireDate { get; set; }
    }

    #endregion

}
