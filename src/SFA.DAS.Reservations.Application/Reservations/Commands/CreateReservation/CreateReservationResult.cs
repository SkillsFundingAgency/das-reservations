﻿using System;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation
{
    public class CreateReservationResult
    {
        public Guid Id { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string CohortRef { get; set; }
        public bool IsEmptyCohortFromSelect { get; set; }
        public uint? ProviderId { get; set; }
        public string JourneyData { get; set; }
    }
}