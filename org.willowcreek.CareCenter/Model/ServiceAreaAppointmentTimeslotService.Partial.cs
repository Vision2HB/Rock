using System.Linq;

using Rock;

namespace org.willowcreek.CareCenter.Model
{
    /// <summary>
    /// Service Area Appointment Time Slot Service
    /// </summary>
    /// <seealso cref="Rock.Data.Service{org.willowcreek.CareCenter.Model.ServiceAreaAppointmentTimeslot}" />
    public partial class ServiceAreaAppointmentTimeslotService
    {

        /// <summary>
        /// Returns an enumerable collection of <see cref="org.willowcreek.CareCenter.Model.ServiceAreaAppointmentTimeslot" /> entities by the Id of the <see cref="org.willowcreek.CareCenter.Model.ServiceAreaAppointmentTimeslot" />
        /// </summary>
        /// <param name="serviceAreaId">The ServiceArea identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="org.willowcreek.CareCenter.Model.ServiceAreaAppointmentTimeslot" /> entities where the Id of the <see cref="org.willowcreek.CareCenter.Model.ServiceAreaAppointmentTimeslot" /> matches the provided value.
        /// </returns>
        public IQueryable<ServiceAreaAppointmentTimeslot> GetByServiceAreaId( int serviceAreaId )
        {
            return Queryable().Where( t => t.ServiceAreaId == serviceAreaId );
        }

    }
}
