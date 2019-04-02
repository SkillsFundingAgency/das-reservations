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

            // id

            if (command.Id == Guid.Empty)
            {
                result.AddError(nameof(command.Id));
            }

            // account id

            if (command.AccountId == default(long))
            {
                result.AddError(nameof(command.AccountId));
            }

            // account legal entity id

            if (command.AccountLegalEntityId == default(long))
            {
                result.AddError(nameof(command.AccountLegalEntityId));
            }

            // account legal entity name

            if (string.IsNullOrWhiteSpace(command.AccountLegalEntityName))
            {
                result.AddError(nameof(command.AccountLegalEntityName));
            }

            // account legal entity public hashed id

            if (string.IsNullOrWhiteSpace(command.AccountLegalEntityPublicHashedId))
            {
                result.AddError(nameof(command.AccountLegalEntityPublicHashedId));
            }

            return Task.FromResult(result);
        }
    }
}