using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation
{
    public class DeleteReservationCommandHandler : IRequestHandler<DeleteReservationCommand>
    {
        private readonly IValidator<DeleteReservationCommand> _validator;

        public DeleteReservationCommandHandler(
            IValidator<DeleteReservationCommand> validator)
        {
            _validator = validator;
        }

        public async Task<Unit> Handle(DeleteReservationCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            // call apiClient

            return Unit.Value;
        }
    }
}