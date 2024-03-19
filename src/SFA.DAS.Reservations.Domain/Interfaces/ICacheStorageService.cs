using System;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Domain.Interfaces;

public interface ICacheStorageService
{
    Task<T> SafeRetrieveFromCache<T>(string key);
    Task<T> RetrieveFromCache<T>(string key);
    Task SaveToCache<T>(string key, T item, TimeSpan expirationInHours);
    Task DeleteFromCache(string key);
}