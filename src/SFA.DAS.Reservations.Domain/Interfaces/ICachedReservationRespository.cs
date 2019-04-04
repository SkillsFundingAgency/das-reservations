using System;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface ICachedReservationRespository
    {
        CachedReservation GetReservation(Guid id);
    }
}
