using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Exceptions;

namespace SFA.DAS.Reservations.Infrastructure.Repositories;

public class CachedReservationRepository(
    ICacheStorageService cacheStorage,
    IReservationAuthorisationService authorisationService)
    : ICachedReservationRespository
{
    public async Task<CachedReservation> GetProviderReservation(Guid id, uint ukPrn)
    {
        ValidateArgs(id, ukPrn);

        var cachedReservation = await cacheStorage.RetrieveFromCache<CachedReservation>(id.ToString());

        if (cachedReservation == null)
        {
            throw new CachedReservationNotFoundException(id);
        }

        if (!authorisationService.ProviderReservationAccessAllowed(ukPrn, cachedReservation))
        {
            throw new UnauthorizedAccessException();
        }

        return cachedReservation;
    }

    private static void ValidateArgs(Guid id, uint ukPrn)
    {
        if (string.IsNullOrEmpty(id.ToString()) || id == Guid.Empty)
        {
            throw new ArgumentException("GUID is not set",nameof(id));
        }

        if (ukPrn == default)
        {
            throw new ArgumentException("ukPrn is not set", nameof(ukPrn));
        }
    }

    public async Task<CachedReservation> GetEmployerReservation(Guid id)
    {
        //TODO: Replace this code with authorisation checking code when we start employer story
        var reservation = await cacheStorage.RetrieveFromCache<CachedReservation>(id.ToString());

        if (reservation == null)
        {
            throw new CachedReservationNotFoundException(id);
        }

        return reservation;
    }
}