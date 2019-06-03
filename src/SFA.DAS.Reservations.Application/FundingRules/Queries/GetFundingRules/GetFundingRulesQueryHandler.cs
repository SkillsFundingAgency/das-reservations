using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules
{
    public class GetFundingRulesQueryHandler : IRequestHandler<GetFundingRulesQuery, GetFundingRulesResult>
    {
        private readonly IFundingRulesService _service;

        public GetFundingRulesQueryHandler(IFundingRulesService service)
        {
            _service = service;
        }

        public async Task<GetFundingRulesResult> Handle(GetFundingRulesQuery request, CancellationToken cancellationToken)
        {
            var result = new GetFundingRulesResult();

            var rules = await _service.GetFundingRules();
            result.FundingRules = rules;

            if (rules?.GlobalRules != null && rules.GlobalRules.Any(x => x != null))
            {
                result.ActiveRule = rules.GlobalRules.First().RuleType;
            }

            return result;



        }
    }
}
