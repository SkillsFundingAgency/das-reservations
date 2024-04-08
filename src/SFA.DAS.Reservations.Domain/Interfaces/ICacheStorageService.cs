using System;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Domain.Interfaces;

public interface ICacheStorageService
{
    Task<T> RetrieveFromCache<T>(string key);
    Task SaveToCache<T>(string key, T item, int expirationInHours);
    Task SaveToCache<T>(string key, T item, TimeSpan timeSpan);
    Task DeleteFromCache(string key);
    Task<T> SafeRetrieveFromCache<T>(string key);
}