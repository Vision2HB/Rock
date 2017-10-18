using System;
using System.Data.Entity;
using System.Linq;

using Rock;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Visit Service
    /// </summary>
    /// <seealso cref="Rock.Data.Service{org.willowcreek.CareCenter.Model.ServiceAreaBan}" />
    public partial class ServiceAreaBanService
    {

        /// <summary>
        /// Returns an enumerable collection of <see cref="org.willowcreek.CareCenter.Model.ServiceAreaBan" /> entities by the Id of the <see cref="org.willowcreek.CareCenter.Model.ServiceArea" />
        /// </summary>
        /// <param name="serviceAreaId">The ServiceArea identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="org.willowcreek.CareCenter.Model.ServiceAreaBan" /> entities where the Id of the <see cref="org.willowcreek.CareCenter.Model.ServiceArea" /> matches the provided value.
        /// </returns>
        public IQueryable<ServiceAreaBan> GetByServiceAreaId( int serviceAreaId )
        {
            return Queryable().Where( t => t.ServiceAreaId == serviceAreaId );
        }

        /// <summary>
        /// Gets the ban expiration date.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="serviceAreaGuid">The service area unique identifier.</param>
        /// <returns></returns>
        public DateTime? GetBanExpirationDate( int personId, string serviceAreaGuid )
        {
            Guid guid = serviceAreaGuid.AsGuid();
            return GetBanExpirationDate( personId, guid );
        }

        /// <summary>
        /// Gets the ban expiration date.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="serviceAreaGuid">The service area unique identifier.</param>
        /// <returns></returns>
        public DateTime? GetBanExpirationDate( int personId, Guid serviceAreaGuid )
        {
            Guid globalServiceAreaGuid = org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_CARE_CENTER_GLOBAL.AsGuid();

            DateTime today = RockDateTime.Today;

            var lastActiveBan = Queryable().AsNoTracking()
                .Where( b =>
                    b.PersonAlias != null &&
                    b.PersonAlias.PersonId == personId &&
                    b.ServiceArea != null &&
                    ( b.ServiceArea.Guid == serviceAreaGuid || b.ServiceArea.Guid == globalServiceAreaGuid ) && 
                    b.BanExpireDate >= today )
                .OrderByDescending( b => b.BanExpireDate )
                .FirstOrDefault();

            return lastActiveBan != null ? lastActiveBan.BanExpireDate : (DateTime?)null;
        }

        /// <summary>
        /// Gets the ban expiration date.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="serviceAreaGuid">The service area unique identifier.</param>
        /// <returns></returns>
        public DateTime? GetBanExpirationDate( int personId, int serviceAreaId )
        {
            Guid globalServiceAreaGuid = org.willowcreek.CareCenter.SystemGuid.ServiceArea.SERVICEAREA_CARE_CENTER_GLOBAL.AsGuid();

            DateTime today = RockDateTime.Today;

            var lastActiveBan = Queryable().AsNoTracking()
                .Where( b =>
                    b.PersonAlias != null &&
                    b.PersonAlias.PersonId == personId &&
                    b.ServiceArea != null &&
                    ( b.ServiceArea.Id == serviceAreaId || b.ServiceArea.Guid == globalServiceAreaGuid ) &&
                    b.BanExpireDate >= today )
                .OrderByDescending( b => b.BanExpireDate )
                .FirstOrDefault();

            return lastActiveBan != null ? lastActiveBan.BanExpireDate : (DateTime?)null;
        }
    }
}
