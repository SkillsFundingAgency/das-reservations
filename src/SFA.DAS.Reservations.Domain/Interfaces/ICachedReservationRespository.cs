using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface ICachedReservationRespository
    {
        Task<CachedReservation> GetProviderReservation(Guid id, uint ukPrn);

        Task<CachedReservation> GetEmployerReservation(Guid id);
    }
}
