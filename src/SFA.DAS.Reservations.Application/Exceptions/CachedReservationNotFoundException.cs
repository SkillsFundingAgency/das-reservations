using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class CachedReservationNotFoundException : Exception
    {
        private readonly Guid _reservationId;

        public CachedReservationNotFoundException(Guid reservationId)
        {
            _reservationId = reservationId;
        }
    }
}
