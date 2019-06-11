using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate
{
    public class CacheReservationStartDateCommandValidator : IValidator<CacheReservationStartDateCommand>
    {
        public Task<ValidationResult> ValidateAsync(CacheReservationStartDateCommand command)
        {
            var result = new ValidationResult();

            if (command.Id == Guid.Empty)
            {
                result.AddError(nameof(command.Id));
            }
            
            if (command.TrainingDate == null)
            {
                result.AddError(nameof(command.TrainingDate));
            }
            else if (command.TrainingDate.StartDate == DateTime.MinValue)
            {
                result.AddError(nameof(command.TrainingDate), $"{nameof(command.TrainingDate.StartDate)} must be set on {nameof(command.TrainingDate)}");
            }

            return Task.FromResult(result);
        }
    }
}