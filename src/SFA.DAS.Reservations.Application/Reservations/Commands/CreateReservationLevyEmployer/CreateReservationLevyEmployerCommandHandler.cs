using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer
{
    public class CreateReservationLevyEmployerCommandHandler : IRequestHandler<CreateReservationLevyEmployerCommand, Unit>
    {
        private readonly IValidator<CreateReservationLevyEmployerCommand> _validator;

        public CreateReservationLevyEmployerCommandHandler(
            IValidator<CreateReservationLevyEmployerCommand> validator)
        {
            _validator = validator;
        }
        public Task<Unit> Handle(CreateReservationLevyEmployerCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
