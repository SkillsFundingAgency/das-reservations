using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand
{
    public class GetProviderCacheReservationCommandQueryValidator : IValidator<GetProviderCacheReservationCommandQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetProviderCacheReservationCommandQuery query)
        {
            var validationResult = new ValidationResult();

            if (query.UkPrn == default(uint))
            {
                validationResult.AddError(nameof(query.UkPrn));
            }

            if (string.IsNullOrEmpty(query.AccountLegalEntityPublicHashedId))
            {
                validationResult.AddError(nameof(query.AccountLegalEntityPublicHashedId));
            }

            if (string.IsNullOrEmpty(query.CohortRef))
            {
                validationResult.AddError(nameof(query.CohortRef));
            }

            return Task.FromResult(validationResult);
        }
    }
}
