using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Services;

public class CacheStorageService : ICacheStorageService
{
    private readonly IDistributedCache _distributedCache;

    /// <summary>
    /// Returns NULL instead of throwing exception if cached item not found.
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> SafeRetrieveFromCache<T>(string key)
    {
        var json = await _distributedCache.GetStringAsync(key);

        return json == null ? default : JsonConvert.DeserializeObject<T>(json);
    }

    public CacheStorageService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task SaveToCache<T>(string key, T item, TimeSpan expirationTimeSpan)
    {
        var json = JsonConvert.SerializeObject(item);

        await _distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationTimeSpan
        });
    }
        
    public async Task SaveToCache<T>(string key, T item, int expirationInHours)
    {
        await SaveToCache(key, item, TimeSpan.FromHours(expirationInHours));
    }


    public async Task<T> RetrieveFromCache<T>(string key)
    {
        var json = await _distributedCache.GetStringAsync(key);
        return json == null ? default : JsonConvert.DeserializeObject<T>(json);
    }

    public async Task DeleteFromCache(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }
}