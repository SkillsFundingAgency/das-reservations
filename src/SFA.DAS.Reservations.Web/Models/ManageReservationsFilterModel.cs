using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ManageReservationsFilterModel
    {
        public string SearchTerm { get; set; }

        public static implicit operator ReservationFilter(ManageReservationsFilterModel source)
        {
            return new ReservationFilter
            {
                SearchTerm = source.SearchTerm
            };
        }
    }
}