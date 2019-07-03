using System;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation
{
    public class CreateReservationResult
    {
        public Guid Id { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string CohortRef { get; set; }
    }
}