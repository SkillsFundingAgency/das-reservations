using System;

namespace SFA.DAS.Reservations.Domain.ReservationsApi
{
    public class CreateReservationApiRequest
    {
        public long AccountId { get; set; }
        public DateTime StartDate { get; set; }
    }
}