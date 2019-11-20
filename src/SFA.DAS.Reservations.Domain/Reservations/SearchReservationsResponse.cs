using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Reservations
{
    public class SearchReservationsResponse
    {
        public IEnumerable<Reservation> Reservations { get; set; }
        public int NumberOfRecordsFound { get; set; }
        public IEnumerable<string> EmployerFilters { get; set; } = new List<string>();
        public IEnumerable<string> CourseFilters { get; set; } = new List<string>();
        public IEnumerable<string> StartDateFilters { get; set; } = new List<string>();
        public int TotalReservationsForProvider { get; set; }
    }
}