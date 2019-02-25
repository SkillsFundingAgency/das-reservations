using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetReservationCommand : IRequest<GetReservationResult>
    {
        public Guid Id { get; set; }
        public string AccountId { get; set; }
    }
}