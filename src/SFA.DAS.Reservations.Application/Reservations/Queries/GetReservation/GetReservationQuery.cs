using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation
{
    public class GetReservationQuery : IRequest<GetReservationResult>
    {
        public Guid Id { get; set; }
    }
}