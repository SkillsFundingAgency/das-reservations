using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules
{
    public class GetAccountFundingRulesQueryHandler : IRequestHandler<GetAccountFundingRulesQuery, GetAccountFundingRulesResult>
    {
        private readonly IFundingRulesService _fundingRulesService;
        private readonly IValidator<GetAccountFundingRulesQuery> _validator;

        public GetAccountFundingRulesQueryHandler(IFundingRulesService fundingRulesService, IValidator<GetAccountFundingRulesQuery> validator)
        {
            _fundingRulesService = fundingRulesService;
            _validator = validator;
        }

        public async Task<GetAccountFundingRulesResult> Handle(GetAccountFundingRulesQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var result = new GetAccountFundingRulesResult();

            var rules = await _fundingRulesService.GetAccountFundingRules(request.AccountId);
            result.AccountFundingRules = rules;

            if (rules?.GlobalRules != null && rules.GlobalRules.Any(x => x != null))
            {
                result.ActiveRule = rules.GlobalRules.First(x=> x != null).RuleType;
            }

            return result;

        }
    }
}
