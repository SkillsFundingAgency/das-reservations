using System;
using MediatR;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetCachedReservationQuery : IReservationQuery, IRequest<GetCachedReservationResult>
    {
        public Guid Id { get; set; }
    }
}