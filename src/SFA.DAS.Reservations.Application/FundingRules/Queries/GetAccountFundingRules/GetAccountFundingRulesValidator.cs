using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules
{
    public class GetAccountFundingRulesValidator : IValidator<GetAccountFundingRulesQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetAccountFundingRulesQuery query)
        {
            var result = new ValidationResult();

            if (query.AccountId < 1 || query.AccountId == default(long))
            {
                result.AddError(nameof(query.AccountId));
            }

            return Task.FromResult(result);
        }
    }
    
}
