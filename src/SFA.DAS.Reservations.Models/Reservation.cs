using System;

namespace SFA.DAS.Reservations.Models
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
    }
}