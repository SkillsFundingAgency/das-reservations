using System;

namespace SFA.DAS.Reservations.Domain.Reservations
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AccountLegalEntityName { get; set; }
    }
}