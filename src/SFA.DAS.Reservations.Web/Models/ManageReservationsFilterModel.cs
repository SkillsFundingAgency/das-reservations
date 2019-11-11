using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ManageReservationsFilterModel
    {
        public string SearchTerm { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PagedRecordsFrom => (PageNumber - 1) * PageSize + 1;
        public int PagedRecordsTo {
            get
            {
                var potentialValue = PageNumber * PageSize + 1;
                return TotalNumberOfRecords < potentialValue ? TotalNumberOfRecords: potentialValue;
            }
        }
        public int TotalNumberOfRecords { get; set; }

        public static implicit operator ReservationFilter(ManageReservationsFilterModel source)
        {
            return new ReservationFilter
            {
                SearchTerm = source.SearchTerm
            };
        }
    }
}