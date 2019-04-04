using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Infrastructure.Services
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
            if (reservation == null || reservation.UkPrn == default(uint))
            {
                throw new ArgumentException("CachedReservation is null",nameof(reservation));
            }

            if (ukPrn == default(uint))
            {
                throw new ArgumentException("ukPrn is not set",nameof(ukPrn));
            }

            return ukPrn == reservation.UkPrn;

        }

        public async Task<bool> ProviderReservationAccessAllowed(uint ukPrn, GetReservationResponse reservation)
        {
            if (reservation == null || reservation.UkPrn == default(uint))
            {
                throw new ArgumentException("GetReservationResponse is null", nameof(reservation));
            }

            if (ukPrn == default(uint))
            {
                throw new ArgumentException("ukPrn is not set", nameof(ukPrn));
            }

            if (ukPrn != reservation.UkPrn)
            {
                return false;
            }

            var trustedList = await _providerPermissionsService.GetTrustedEmployers(ukPrn);

            if (trustedList.Any(e => e.AccountLegalEntityId == reservation.AccountLegalEntityId))
            {
                return true;
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
