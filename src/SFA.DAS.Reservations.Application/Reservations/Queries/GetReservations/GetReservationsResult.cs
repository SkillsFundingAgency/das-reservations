using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations
{
    public class GetReservationsResult
    {
        public IEnumerable<Reservation> Reservations { get; set; }
    }
}