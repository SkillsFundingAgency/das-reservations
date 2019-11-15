using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations
{
    public class SearchReservationsResult
    {
        public IEnumerable<Reservation> Reservations { get; set; }
        public int NumberOfRecordsFound { get; set; }
        public IEnumerable<string> CourseFilters { get; set; }

        public static implicit operator SearchReservationsResult(SearchReservationsResponse source)
        {
            return new SearchReservationsResult
            {
                Reservations = source.Reservations,
                NumberOfRecordsFound = source.NumberOfRecordsFound,
                CourseFilters = source.CourseFilters
            };
        }
    }
}