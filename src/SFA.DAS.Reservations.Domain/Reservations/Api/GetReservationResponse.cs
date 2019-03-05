using System;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class GetReservationResponse
    {
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}