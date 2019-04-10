using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation
{
    public class GetCachedReservationQuery : IReservationQuery, IRequest<GetCachedReservationResult>
    {
        public Guid Id { get; set; }
        public uint UkPrn { get; set; }
    }
}