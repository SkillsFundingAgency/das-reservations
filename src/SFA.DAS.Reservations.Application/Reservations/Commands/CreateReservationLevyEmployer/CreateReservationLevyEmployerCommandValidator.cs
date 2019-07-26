using System;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer
{
    public class CreateReservationLevyEmployerCommandValidator : IValidator<CreateReservationLevyEmployerCommand>
    {
        public Task<ValidationResult> ValidateAsync(CreateReservationLevyEmployerCommand query)
        {
            var validationResult = new ValidationResult();
            
            if (query.AccountId < 1)
            {
                validationResult.AddError(nameof(query.AccountId));
            }

            if (query.AccountLegalEntityId < 1)
            {
                validationResult.AddError(nameof(query.AccountLegalEntityId));
            }
            
            return Task.FromResult(validationResult);
        }
    }
}
