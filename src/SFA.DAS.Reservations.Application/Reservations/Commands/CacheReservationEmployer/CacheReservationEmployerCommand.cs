﻿using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer
{
    public class CacheReservationEmployerCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public uint? UkPrn { get; set; }
        public string AccountName { get; set; }
        public string CohortRef { get; set; }
        public bool EmployerHasSingleLegalEntity  { get; set; }
        public bool IsEmptyCohortFromSelect { get; set; }
        public string JourneyData { get; set; }
        public bool CreateViaAutoReservation { get; set; }
    }
}