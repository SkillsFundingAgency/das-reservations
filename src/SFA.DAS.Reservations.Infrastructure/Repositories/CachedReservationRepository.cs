using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
<<<<<<< Updated upstream
=======
using SFA.DAS.Reservations.Domain.Reservations.Api;
>>>>>>> Stashed changes
using SFA.DAS.Reservations.Infrastructure.Exceptions;

namespace SFA.DAS.Reservations.Infrastructure.Repositories
{
    public class CachedReservationRepository : ICachedReservationRespository
    {
        private readonly ICacheStorageService _cacheStorage;
        private readonly IReservationAuthorisationService _authorisationService;

        public CachedReservationRepository(ICacheStorageService cacheStorage,
            IReservationAuthorisationService authorisationService)
        {
            _cacheStorage = cacheStorage;
            _authorisationService = authorisationService;
        }

        public async Task<CachedReservation> GetProviderReservation(Guid id, uint ukPrn)
        {
            if (string.IsNullOrEmpty(id.ToString()) || id == Guid.Empty)
            {
                throw new ArgumentException("GUID is not set",nameof(id));
            }

            if (ukPrn == default(uint))
            {
                throw new ArgumentException("ukPrn is not set", nameof(ukPrn));
            }

            var cachedReservation = await _cacheStorage.RetrieveFromCache<CachedReservation>(id.ToString());

            if (cachedReservation == null)
            {
                throw new CachedReservationNotFoundException(id);
            }

            if (!_authorisationService.ProviderReservationAccessAllowed(ukPrn, cachedReservation))
            {
                throw new UnauthorizedAccessException();
            }

            return cachedReservation;
        }

        public async Task<CachedReservation> GetEmployerReservation(Guid id)
        {
            //TODO: Replace this code with authorisation checking code when we start employer story
            var reservation = await _cacheStorage.RetrieveFromCache<CachedReservation>(id.ToString());

            if (reservation == null)
            {
                throw new CachedReservationNotFoundException(id);
            }

            return reservation;
        }
    }
}
