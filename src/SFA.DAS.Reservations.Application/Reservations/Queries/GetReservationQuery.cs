using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetReservationQuery : IReservationQuery, IRequest<GetReservationResult>
    {
        public Guid Id { get; set; }
    }
}