using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation
{
    public class CreateReservationCommand : IRequest<CreateReservationResult>
    {
        public Guid Id { get; set; }
        public uint? UkPrn { get; set; }
    }
}