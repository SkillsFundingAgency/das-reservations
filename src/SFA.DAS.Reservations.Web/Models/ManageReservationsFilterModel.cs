using System.Collections;
using System.Collections.Generic;
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
        public List<PageLink> PageLinks { get; set; }

        public static implicit operator ReservationFilter(ManageReservationsFilterModel source)
        {
            return new ReservationFilter
            {
                SearchTerm = source.SearchTerm
            };
        }
    }

    public class PageLink
    {
        public string Label { get; set; }
        public string AriaLabel { get; set; }
        public bool? IsCurrent { get; set; }
        public string Link { get; set; }
    }
}