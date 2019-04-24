using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations
{
    public class GetReservationsQueryHandler : IRequestHandler<GetReservationsQuery, GetReservationsResult>
    {
        public Task<GetReservationsResult> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new GetReservationsResult{Reservations = new List<Reservation>()});
        }
    }
}