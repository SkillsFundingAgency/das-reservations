using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationValidator : IValidator<CreateReservationCommand>
    {
        public Task<ValidationResult> ValidateAsync(CreateReservationCommand item)
        {
            var result = new ValidationResult();

            if (item.AccountId < 1)
            {
                result.AddError(nameof(item.AccountId));
            }

            if (item.StartDate == DateTime.MinValue)
            {
                result.AddError(nameof(item.StartDate));
            }

            return Task.FromResult(result);
        }
    }
}