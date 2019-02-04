using System;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationApiRequest
    {
        public long AccountId { get; set; }
        public DateTime StartDate { get; set; }
    }
}