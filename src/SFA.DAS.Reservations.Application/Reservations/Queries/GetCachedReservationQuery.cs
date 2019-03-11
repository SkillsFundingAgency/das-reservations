using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetCachedReservationQuery : IRequest<GetCachedReservationResult>
    {
        public Guid Id { get; set; }
    }
}