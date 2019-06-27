using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> GetReservations(long accountId);
        Task<CreateReservationResponse> CreateReservationLevyEmployer(
            Guid reservationId,
            long accountId,
            long accountLegalEntityId);
    }
}