using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer
{
    public class CacheReservationEmployerCommandValidator : IValidator<CacheReservationEmployerCommand>
    {

        public Task<ValidationResult> ValidateAsync(CacheReservationEmployerCommand command)
        {
            var result = new ValidationResult();

            if (command.Id == Guid.Empty)
            {
                result.AddError(nameof(command.Id));
            }

            if (command.AccountId == default(long))
            {
                result.AddError(nameof(command.AccountId));
            }

            if (command.AccountLegalEntityId == default(long))
            {
                result.AddError(nameof(command.AccountLegalEntityId));
            }

            if (string.IsNullOrWhiteSpace(command.AccountLegalEntityName))
            {
                result.AddError(nameof(command.AccountLegalEntityName));
            }

            if (string.IsNullOrWhiteSpace(command.AccountLegalEntityPublicHashedId))
            {
                result.AddError(nameof(command.AccountLegalEntityPublicHashedId));
            }

            return Task.FromResult(result);
        }
    }
}