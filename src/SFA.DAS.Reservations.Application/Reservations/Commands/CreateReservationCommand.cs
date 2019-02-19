using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommand : IRequest<CreateReservationResult>
    {
        public string AccountId { get; set; }
        public string StartDate { get; set; }
    }
}