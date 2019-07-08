using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations
{
    public class GetAvailableReservationsResult
    {
        public IEnumerable<Reservation> Reservations { get; set; }
    }
}