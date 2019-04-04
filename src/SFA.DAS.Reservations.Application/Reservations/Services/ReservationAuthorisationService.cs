using System;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public class ReservationAuthorisationService : IReservationAuthorisationService
    {
        public bool ProviderReservationAccessAllowed(uint ukPrn, CachedReservation reservation)
        {
            throw new NotImplementedException();
        }
    }
}
