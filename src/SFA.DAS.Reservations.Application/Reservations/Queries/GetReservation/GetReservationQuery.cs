using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation
{
    public class GetReservationQuery : IReservationQuery, IRequest<GetReservationResult>
    {
        public Guid Id { get; set; }
        public uint UkPrn { get; set; }
    }
}