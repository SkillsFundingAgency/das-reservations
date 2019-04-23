using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation
{
    public class CreateReservationCommandValidator : IValidator<CreateReservationCommand>
    {
        public Task<ValidationResult> ValidateAsync(CreateReservationCommand command)
        {
            var result = new ValidationResult();

            if (command.Id == Guid.Empty)
            {
                result.AddError(nameof(command.Id));
            }

            return Task.FromResult(result);
        }
    }
}