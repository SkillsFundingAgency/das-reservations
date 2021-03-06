﻿using System;

namespace SFA.DAS.Reservations.Infrastructure.Exceptions
{
    public class CachedReservationNotFoundException : Exception
    {
        public Guid ReservationId { get; }

        public CachedReservationNotFoundException(Guid reservationId) 
            : base($"No reservation was found with id [{reservationId}].")
        {
            ReservationId = reservationId;
        }
    }
}
