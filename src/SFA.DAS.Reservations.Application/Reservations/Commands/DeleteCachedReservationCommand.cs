using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class DeleteCachedReservationCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}