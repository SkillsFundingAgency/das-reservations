using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheReservationCommandHandler : IRequestHandler<CacheReservationCommand, CacheReservationResult>
    {
        private readonly IValidator<BaseCreateReservationCommand> _validator;

        public CacheReservationCommandHandler(IValidator<BaseCreateReservationCommand> validator)
        {
            _validator = validator;
        }

        public async Task<CacheReservationResult> Handle(CacheReservationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var result = new CacheReservationResult();
            await Task.CompletedTask;
            return result;
        }
    }
}