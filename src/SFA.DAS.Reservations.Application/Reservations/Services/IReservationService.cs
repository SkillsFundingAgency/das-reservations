using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> GetReservations(long accountId);
    }
}