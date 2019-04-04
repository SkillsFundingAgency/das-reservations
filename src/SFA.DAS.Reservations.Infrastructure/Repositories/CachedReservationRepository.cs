using System;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Infrastructure.Repositories
{
    public class CachedReservationRepository : ICachedReservationRespository
    {
        private readonly ICacheStorageService _cacheStorage;

        public CachedReservationRepository(ICacheStorageService cacheStorage)
        {
            _cacheStorage = cacheStorage;
        }
        public CachedReservation GetReservation(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
