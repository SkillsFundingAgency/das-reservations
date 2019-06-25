using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer
{
    public class CreateReservationLevyEmployerCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public uint UkPrn { get; set; }
        public bool IsLevy { get; set; }
    }
}
