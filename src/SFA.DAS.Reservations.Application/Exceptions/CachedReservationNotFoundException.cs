using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class CachedReservationNotFoundException : Exception
    {
        public Guid ReservationId { get; }

        public CachedReservationNotFoundException(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}
