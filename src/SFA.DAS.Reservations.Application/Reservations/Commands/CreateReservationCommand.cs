using System;
using MediatR;
using SFA.DAS.Reservations.Application.Reservations.Queries;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommand : IReservationQuery, IRequest<CreateReservationResult>
    {
        public Guid Id { get; set; }
    }
}