using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetCachedReservationQuery : IReservationQuery, IRequest<GetCachedReservationResult>
    {
        public Guid Id { get; set; }
    }
}