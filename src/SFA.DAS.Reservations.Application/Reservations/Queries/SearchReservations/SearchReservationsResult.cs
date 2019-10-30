using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations
{
    public class SearchReservationsResult
    {
        public IEnumerable<Reservation> Reservations { get; set; }
        public int NumberOfRecordsFound { get; set; }
    }
}