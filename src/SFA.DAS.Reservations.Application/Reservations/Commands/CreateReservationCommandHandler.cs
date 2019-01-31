using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IValidator<CreateReservationCommand> _validator;

        public CreateReservationCommandHandler(IValidator<CreateReservationCommand> validator)
        {
            _validator = validator;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAsync(request);

            return new CreateReservationResult();
        }
    }
}