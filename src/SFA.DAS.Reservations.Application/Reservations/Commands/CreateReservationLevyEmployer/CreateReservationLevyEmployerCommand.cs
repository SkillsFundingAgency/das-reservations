using System;
using System.Text;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer
{
    public class CreateReservationLevyEmployerCommand : IRequest<CreateReservationLevyEmployerResult>
    {
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
    }
}
