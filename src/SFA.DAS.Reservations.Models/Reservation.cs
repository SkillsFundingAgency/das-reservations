using System;

namespace SFA.DAS.Reservations.Models
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public DateTime StartDate { get; set; }
    }
}