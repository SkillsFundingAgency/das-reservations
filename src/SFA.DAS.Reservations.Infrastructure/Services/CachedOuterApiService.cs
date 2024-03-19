using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Services;

public interface ICachedOuterApiService
{
    Task<bool> CanAccessCohort(Party party, long partyId, long cohortId);
}

public class CachedOuterApiService(ICacheStorageService cacheStorage, IReservationsOuterService outerApiService) : ICachedOuterApiService
{
    public const int CacheExpirationMinutes = 5;

    public async Task<bool> CanAccessCohort(Party party, long partyId, long cohortId)
    {
        var cacheKey = $"{nameof(CanAccessCohort)}.{party}.{partyId}.{cohortId}";
        var cachedResponse = await cacheStorage.SafeRetrieveFromCache<bool?>(cacheKey);

        if (cachedResponse != null)
        {
            return cachedResponse.Value;
        }

        var canAccessCohort = await outerApiService.CanAccessCohort(party, partyId, cohortId);

        await cacheStorage.SaveToCache(cacheKey, canAccessCohort, TimeSpan.FromMinutes(CacheExpirationMinutes));

        return canAccessCohort;
    }
}