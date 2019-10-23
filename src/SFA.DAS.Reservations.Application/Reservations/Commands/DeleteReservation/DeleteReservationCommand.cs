using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation
{
    public class DeleteReservationCommand : IRequest
    {
        public Guid ReservationId { get; set; }
        public bool DeletedByEmployer { get; set; }
    }
}