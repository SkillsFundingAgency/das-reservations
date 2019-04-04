using System;
using SFA.DAS.Reservations.Application.Employers.Services;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public class ReservationAuthorisationService : IReservationAuthorisationService
    {
        private readonly IProviderPermissionsService _providerPermissionsService;

        public ReservationAuthorisationService(IProviderPermissionsService providerPermissionsService)
        {
            _providerPermissionsService = providerPermissionsService;
        }

        public bool ProviderReservationAccessAllowed(uint ukPrn, CachedReservation reservation)
        {
            throw new NotImplementedException();
        }

        public bool ProviderReservationAccessAllowed(uint ukPrn, GetReservationResponse reservation)
        {
            throw new NotImplementedException();
        }
    }
}
