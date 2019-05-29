using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextActiveGlobalFundingRule
{
    public class GetNextActiveGlobalFundingRuleQueryHandler : IRequestHandler<GetNextActiveGlobalFundingRuleQuery, GetNextActiveGlobalFundingRuleResult>
    {
        private readonly IFundingRulesService _service;

        public GetNextActiveGlobalFundingRuleQueryHandler(IFundingRulesService service)
        {
            _service = service;
        }

        public async Task<GetNextActiveGlobalFundingRuleResult> Handle(GetNextActiveGlobalFundingRuleQuery request, CancellationToken cancellationToken)
        {
            var response = await _service.GetFundingRules();

            var nextRule = response?.GlobalRules?.Where(rule => rule.ActiveFrom.HasValue && rule.ActiveFrom >= DateTime.Now)
                .OrderBy(r => r.ActiveFrom)
                .FirstOrDefault();
            
            return new GetNextActiveGlobalFundingRuleResult{ Rule = nextRule };
        }
    }
}
