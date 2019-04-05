using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;

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
    }
}
