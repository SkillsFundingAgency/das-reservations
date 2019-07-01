using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount
{
    public class GetAccountLegalEntityQueryValidator : IValidator<GetAccountLegalEntityQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetAccountLegalEntityQuery query)
        {
            var validationResult = new ValidationResult();

            if (string.IsNullOrEmpty(query.AccountLegalEntityPublicHashedId))
            {
                validationResult.AddError(nameof(query.AccountLegalEntityPublicHashedId), $"{nameof(query.AccountLegalEntityPublicHashedId)} has not been supplied");
            }

            return Task.FromResult(validationResult);
        }
    }
}
