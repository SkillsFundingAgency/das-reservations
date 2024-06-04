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
        private readonly IReservationsOuterService _reservationsOuterService;

        public ReservationAuthorisationService(IReservationsOuterService reservationsOuterService)
        {
            _reservationsOuterService = reservationsOuterService;
        }

        public bool ProviderReservationAccessAllowed(uint ukPrn, CachedReservation reservation)
        {
            if (reservation == null)
            {
                throw new ArgumentException("CachedReservation is null", nameof(reservation));
            }

            if (reservation.UkPrn == default(uint))
            {
                throw new ArgumentException("CachedReservation UkPrn is null", nameof(reservation));
            }

            if (ukPrn == default(uint))
            {
                throw new ArgumentException("ukPrn is not set", nameof(ukPrn));
            }

            return ukPrn == reservation.UkPrn;
        }

        public async Task<bool> ProviderReservationAccessAllowed(uint ukPrn, GetReservationResponse reservation)
        {
            if (reservation == null || reservation.ProviderId == default(uint))
            {
                throw new ArgumentException("GetReservationResponse is null", nameof(reservation));
            }

            if (ukPrn == default(uint))
            {
                throw new ArgumentException("ukPrn is not set", nameof(ukPrn));
            }

            if (ukPrn != reservation.ProviderId)
            {
                return false;
            }

            var trustedList = await _reservationsOuterService.GetTrustedEmployers(ukPrn);

            if (trustedList.All(e => e.AccountLegalEntityId != reservation.AccountLegalEntityId))
            {
                throw new UnauthorizedAccessException();
            }

            return true;
        }
    }
}
