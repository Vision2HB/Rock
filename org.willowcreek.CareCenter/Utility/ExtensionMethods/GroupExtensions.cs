using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        #region Group Extensions

        /// <summary>
        /// Gets the income status.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <returns></returns>
        public static IncomeStatus GetIncomeStatus( this Group family )
        {
            decimal? familyIncome = null;
            bool familyAttendsWillow = false;

            if ( family.Attributes == null )
            {
                family.LoadAttributes();
            }

            var householdIncomeAttribute = AttributeCache.Read( SystemGuid.Attribute.FAMILY_HOUSEHOLD_INCOME.AsGuid() );
            if ( householdIncomeAttribute != null )
            {
                familyIncome = family.GetAttributeValue( householdIncomeAttribute.Key ).AsDecimalOrNull();
            }

            var churchStatusAttribute = AttributeCache.Read( SystemGuid.Attribute.FAMILY_CHURCH_STATUS.AsGuid() );
            if ( churchStatusAttribute != null )
            {
                var churchStatusGuid = family.GetAttributeValue( churchStatusAttribute.Key );
                familyAttendsWillow = churchStatusGuid.ToString().ToLower() == ( SystemGuid.DefinedValue.CHURCHSTATUS_ATTENDS_WILLOW ).ToString().ToLower();
            }

            return GetIncomeStatus( family, familyIncome, familyAttendsWillow );

        }

        /// <summary>
        /// Gets the income status.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="familyIncome">The family income.</param>
        /// <param name="familyAttendsWillow">if set to <c>true</c> [family attends willow].</param>
        /// <returns></returns>
        public static IncomeStatus GetIncomeStatus( this Group family, decimal? familyIncome, bool familyAttendsWillow )
        {
            int householdSize = 1;

            var householdSizeAttribute = AttributeCache.Read( SystemGuid.Attribute.FAMILY_HOUSEHOLD_SIZE.AsGuid() );
            var attendeePovertyLevelAttribute = AttributeCache.Read( SystemGuid.Attribute.DEFINEDVALUE_POVERTY_ATTENDER.AsGuid() );
            var nonAttendeePovertyLevelAttribute = AttributeCache.Read( SystemGuid.Attribute.DEFINEDVALUE_POVERTY_NONATTENDER.AsGuid() );

            if (attendeePovertyLevelAttribute  != null && nonAttendeePovertyLevelAttribute != null){

                if ( family.Attributes == null )
                {
                    family.LoadAttributes();
                }

                var familyType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                int adultRoleId = familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) ).Id;
                int childRoleId = familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) ).Id;

                if ( familyIncome.HasValue )
                {
                    int numberOfAdultsInFamily = family.Members.Where( m =>
                                                        m.Person.IsDeceased == false
                                                        && m.GroupRoleId == adultRoleId
                                                   ).Count();

                    // make sure there isn't more than 2 adults
                    numberOfAdultsInFamily = numberOfAdultsInFamily > 2 ? 2 : numberOfAdultsInFamily;

                    // get number of children 18 or younger
                    int numberOfChildren = family.Members.Where( m =>
                                                        m.Person.IsDeceased == false
                                                        && m.GroupRoleId == childRoleId
                                                        && m.Person.Age < 19
                                                   ).Count();


                    householdSize = numberOfAdultsInFamily + numberOfChildren;

                    var povertyGuidlinesDt = DefinedTypeCache.Read( SystemGuid.DefinedType.POVERTY_GUIDELINES.AsGuid() );
                    var povertyLevel = povertyGuidlinesDt.DefinedValues.Where( dv => dv.DefinedTypeId == povertyGuidlinesDt.Id && dv.Value == householdSize.ToString() ).FirstOrDefault();

                    decimal povertyLevelAmount = 0;

                    if( povertyLevel != null )
                    {
                        if ( familyAttendsWillow )
                        {
                            povertyLevelAmount = povertyLevel.GetAttributeValue( attendeePovertyLevelAttribute.Key ).AsDecimal();
                        }
                        else
                        {
                            povertyLevelAmount = povertyLevel.GetAttributeValue( nonAttendeePovertyLevelAttribute.Key ).AsDecimal();
                        }
                    }                    

                    if ( familyIncome < povertyLevelAmount )
                    {
                        // meets income requirements
                        return IncomeStatus.Meets;
                    }
                    else
                    {
                        // does not meet income requirements
                        return IncomeStatus.DoesNotMeet;
                    }
                }
            }

            return IncomeStatus.Unknown;
        }

        /// <summary>
        /// Gets the food visit status.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <returns></returns>
        public static FoodVisitStatus GetFoodVisitStatus( this Group family )
        {
            var foodVisitStatusResponse = new FoodVisitStatus();

            var rockContext = new RockContext();
            var serviceAreaService = new ServiceAreaService( rockContext );

            // get last 10 full food visits for the family (getting 10 so that we can make sure we can check families who are allowed more than 1 visit (doubt they' be allowed 10)).
            var foodVisits = new VisitService( rockContext )
                .GetLastVisits( family.Id, SystemGuid.ServiceArea.SERVICEAREA_FOOD )
                .AsNoTracking()
                .Take( 10 )
                .ToList();

            // get last 10 bread visits for the family
            var breadVisits = new VisitService( rockContext )
                .GetLastVisits( family.Id, SystemGuid.ServiceArea.SERVICEAREA_BREAD )
                .AsNoTracking()
                .Take( 10 )
                .ToList();

            // determine food services taken
            if ( family.Attributes == null )
            {
                family.LoadAttributes();
            }

            var graceVisitAttribute = AttributeCache.Read( SystemGuid.Attribute.FAMILY_GRACE_FOOD_VISIT.AsGuid() );
            if ( graceVisitAttribute != null )
            {
                foodVisitStatusResponse.LastGraceVisitDate = family.GetAttributeValue( graceVisitAttribute.Key ).AsDateTime();
            }
            foodVisitStatusResponse.GraceVisitAllowed = !foodVisitStatusResponse.LastGraceVisitDate.HasValue;

            var courtesyVisitAttribute = AttributeCache.Read( SystemGuid.Attribute.FAMILY_COURTESY_FOOD_VISIT.AsGuid() );
            if ( courtesyVisitAttribute != null )
            {
                foodVisitStatusResponse.LastCourtesyVisitDate = family.GetAttributeValue( courtesyVisitAttribute.Key ).AsDateTime();
            }
            foodVisitStatusResponse.CourtesyVisitAllowed = !foodVisitStatusResponse.LastCourtesyVisitDate.HasValue;

            var foodVisitsPerMonthAttribute = AttributeCache.Read( SystemGuid.Attribute.FAMILY_FOOD_VISITS_PER_MONTH.AsGuid() );
            if ( foodVisitsPerMonthAttribute != null )
            {
                foodVisitStatusResponse.FoodVisitsAllowedPerMonth = family.GetAttributeValue( foodVisitsPerMonthAttribute.Key ).AsInteger();
            }

            var foodVisitsPerMonthExpireDateAttribute = AttributeCache.Read( SystemGuid.Attribute.FAMILY_FOOD_VISITS_OVERRIDE_EXPIRE.AsGuid() );
            if ( foodVisitsPerMonthExpireDateAttribute != null )
            { 
                foodVisitStatusResponse.FoodVisitOverrideExpireDate = family.GetAttributeValue( foodVisitsPerMonthExpireDateAttribute.Key ).AsDateTime();
            }

            // if visit's per month expired set visits allowed back to 1
            if ( foodVisitStatusResponse.FoodVisitOverrideExpireDate.HasValue && RockDateTime.Today > foodVisitStatusResponse.FoodVisitOverrideExpireDate.Value.Date )
            {
                foodVisitStatusResponse.FoodVisitsAllowedPerMonth = 1;
            }

            if ( foodVisits.Any() )
            {
                foodVisitStatusResponse.LastFoodVisit = foodVisits.Select( v => v.VisitDate ).FirstOrDefault();
            }

            if ( breadVisits.Any() )
            {
                foodVisitStatusResponse.LastBreadVisit = breadVisits.Select( v => v.VisitDate ).FirstOrDefault();
            }

            var currentMonth = RockDateTime.Now.Month;
            var currentYear = RockDateTime.Now.Year;

            var currentWeekStart = RockDateTime.Now.SundayDate();
            var currentWeekEnd = currentWeekStart.AddDays( 7 );

            // determine if a food visit is allowed
            foodVisitStatusResponse.FoodVisitAllowed = foodVisits.Where( v =>
                                                             v.VisitDate.Month == currentMonth
                                                             && v.VisitDate.Year == currentYear )
                                                             .Count() < foodVisitStatusResponse.FoodVisitsAllowedPerMonth;

            // determine if a bread visit is allowed (one per calendar week)
            foodVisitStatusResponse.BreadVisitAllowed = !breadVisits.Where( v =>
                                                            v.VisitDate > currentWeekStart
                                                            && v.VisitDate < currentWeekEnd )
                                                            .Any();

            return foodVisitStatusResponse;
        }

        /// <summary>
        /// Gets the clothing visit status.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <returns></returns>
        public static ClothingVisitStatus GetClothingVisitStatus( this Group family )
        {
            var clothingVisitStatusResponse = new ClothingVisitStatus();

            clothingVisitStatusResponse.ClothingLevel = ClothingLevel.Full;     // Give everyone full clothing.
            //clothingVisitStatusResponse.ClothingLevel = ClothingLevel.Limited;

            //if ( family.Members.Where(m => m.Person.Age <= 14 ).Any() )
            //{
            //    clothingVisitStatusResponse.ClothingLevel = ClothingLevel.Full;
            //}
            //else
            //{
            //    // check for override
            //    if ( family.Attributes == null )
            //    {
            //        family.LoadAttributes();
            //    }
            //    var overrideAttr = AttributeCache.Read( SystemGuid.Attribute.FAMILY_CLOTHING_VISIT_OVERRIDE.AsGuid() );
            //    if ( overrideAttr != null && family.GetAttributeValue( overrideAttr.Key ).AsBoolean() )
            //    {
            //        clothingVisitStatusResponse.ClothingLevel = ClothingLevel.Full;
            //    }
            //}

            return clothingVisitStatusResponse;
        }

        public static AutoRepairStatus GetAutoRepairStatus( this Group family )
        {
            var autoRepairStatus = new AutoRepairStatus();

            autoRepairStatus.ReceiveAutoAllowed = true;

            if ( family.Attributes == null )
            {
                family.LoadAttributes();
            }

            var mostRecentCarReceived = AttributeCache.Read( SystemGuid.Attribute.PERSON_CAR_RECEIVED.AsGuid() );
            var mostRecentAutoRepairAttr = AttributeCache.Read( SystemGuid.Attribute.PERSON_MOST_RECENT_AUTO_REPAIR.AsGuid() );
            var previousAutoRepairAttr = AttributeCache.Read( SystemGuid.Attribute.PERSON_PREVIOUS_AUTO_REPAIR.AsGuid() );

            autoRepairStatus.AutoRepairDates = new List<DateTime>();
            foreach( var person in family.Members.Select( m => m.Person ) )
            {
                if ( person.Attributes == null )
                {
                    person.LoadAttributes();
                }

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

                DateTime? mostRecentAutoReceived = person.GetAttributeValue( mostRecentCarReceived.Key ).AsDateTime();
                if ( mostRecentAutoReceived.HasValue )
                {
                    autoRepairStatus.ReceiveAutoAllowed = false;

                    if ( !autoRepairStatus.ReceivedAutoDate.HasValue || autoRepairStatus.ReceivedAutoDate.Value < mostRecentAutoReceived.Value  )
                    {
                        autoRepairStatus.ReceivedAutoDate = mostRecentAutoReceived.Value;
                    }
                }
            }

            autoRepairStatus.AttendsWillow = false;
            var churchStatusAttr = AttributeCache.Read( SystemGuid.Attribute.FAMILY_CHURCH_STATUS.AsGuid() );
            if ( churchStatusAttr != null &&
                family.GetAttributeValue( churchStatusAttr.Key ).AsGuid() == SystemGuid.DefinedValue.CHURCHSTATUS_ATTENDS_WILLOW.AsGuid() )
            {
                autoRepairStatus.AttendsWillow = true;
            }

            return autoRepairStatus;
        }

        /// <summary>
        /// Gets the resource visit status.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <returns></returns>
        public static ResourceVisitStatus GetResourceVisitStatus( this Group family )
        {
            var resourceVisitStatus = new ResourceVisitStatus();

            var rockContext = new RockContext();
            var serviceAreaService = new ServiceAreaService( rockContext );

            // Determine the threshold number from global attribute
            var attr = AttributeCache.Read( SystemGuid.Attribute.GLOBAL_RESOURCE_VISIT_ENABLED_COUNT.AsGuid() );
            if ( attr != null )
            {
                resourceVisitStatus.FoodVisitThreshold = GlobalAttributesCache.Read().GetValue( attr.Key ).AsIntegerOrNull();
            }

            // If a threshhold was set
            if ( resourceVisitStatus.FoodVisitThreshold.HasValue )
            {
                // Get the food visits in the last year
                DateTime yearAgo = RockDateTime.Today.AddYears( -1 );
                resourceVisitStatus.FoodVisits = new VisitService( rockContext )
                    .GetLastVisits( family.Id, SystemGuid.ServiceArea.SERVICEAREA_FOOD )
                    .AsNoTracking()
                    .Where( v => v.VisitDate > yearAgo )
                    .Count();

                // If threshold has been met
                if ( resourceVisitStatus.FoodVisits >= resourceVisitStatus.FoodVisitThreshold )
                {
                    // Check to see if a resource visit has already been complted
                    var lastResourceVisit = new VisitService( rockContext )
                        .GetLastVisits( family.Id, SystemGuid.ServiceArea.SERVICEAREA_RESOURCE )
                        .AsNoTracking()
                        .Where( v => v.VisitDate > yearAgo )
                        .FirstOrDefault();

                    if ( lastResourceVisit == null )
                    {
                        // If not, allow the resource visit
                        resourceVisitStatus.ResourceVisitAllowed = true;
                    }
                    else
                    {
                        // If so, set the date
                        resourceVisitStatus.LastResourceVisit = lastResourceVisit.VisitDate;
                    }
                }
            }

            return resourceVisitStatus;

        }

        #endregion
    }
}
