using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
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

        public Task<CachedReservation> GetProviderReservation(Guid id, uint ukPrn)
        {
            throw new NotImplementedException();
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
