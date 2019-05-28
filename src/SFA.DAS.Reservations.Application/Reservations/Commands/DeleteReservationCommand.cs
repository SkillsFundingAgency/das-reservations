using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class DeleteReservationCommand : IRequest
    {
        public Guid ReservationId { get; set; }
    }
}