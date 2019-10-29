using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Reservations
{
    public class SearchReservationsResponse
    {
        public IEnumerable<Reservation> Reservations { get; set; }
        public int NumberOfRecordsFound { get; set; }
    }
}