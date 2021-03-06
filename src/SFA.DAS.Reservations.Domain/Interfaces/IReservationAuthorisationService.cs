﻿using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IReservationAuthorisationService
    {
        bool ProviderReservationAccessAllowed(uint ukPrn, CachedReservation reservation);
        Task<bool> ProviderReservationAccessAllowed(uint ukPrn, GetReservationResponse reservation);
    }
}
