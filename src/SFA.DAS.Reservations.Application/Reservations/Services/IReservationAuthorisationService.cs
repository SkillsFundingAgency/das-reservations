using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public interface IReservationAuthorisationService
    {
        bool ProviderReservationAccessAllowed(uint ukPrn, CachedReservation reservation);
    }
}