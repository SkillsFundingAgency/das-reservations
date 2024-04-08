using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Services;

public interface ICachedReservationsOuterService
{
    Task<bool> CanAccessCohort(long providerId, long cohortId);
}

public class CachedReservationsOuterService(ICacheStorageService cacheStorage, IReservationsOuterService outerApiService) : ICachedReservationsOuterService
{
    public const int CacheExpirationMinutes = 5;
    
    public async Task<bool> CanAccessCohort(long providerId, long cohortId)
    {
        var cacheKey = $"{nameof(CanAccessCohort)}.{providerId}.{cohortId}";
        var cachedResponse = await cacheStorage.SafeRetrieveFromCache<bool?>(cacheKey);

        if (cachedResponse != null)
        {
            return cachedResponse.Value;
        }

        var canAccessCohort = await outerApiService.CanAccessCohort(providerId, cohortId);

        await cacheStorage.SaveToCache(cacheKey, canAccessCohort,  TimeSpan.FromMinutes(CacheExpirationMinutes));

        return canAccessCohort;
    }
}