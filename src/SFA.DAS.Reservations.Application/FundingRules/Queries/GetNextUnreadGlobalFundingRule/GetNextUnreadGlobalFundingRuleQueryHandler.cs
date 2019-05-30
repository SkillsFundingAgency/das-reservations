using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule
{
    public class GetNextUnreadGlobalFundingRuleQueryHandler : IRequestHandler<GetNextUnreadGlobalFundingRuleQuery, GetNextUnreadGlobalFundingRuleResult>
    {
        private readonly IFundingRulesService _service;

        public GetNextUnreadGlobalFundingRuleQueryHandler(IFundingRulesService service)
        {
            _service = service;
        }

        public async Task<GetNextUnreadGlobalFundingRuleResult> Handle(GetNextUnreadGlobalFundingRuleQuery request, CancellationToken cancellationToken)
        {
            var response = await _service.GetUnreadFundingRules(request.Id);

            var nextRule = response?.GlobalRules?.Where(rule => rule.ActiveFrom.HasValue && rule.ActiveFrom >= DateTime.Now)
                .OrderBy(r => r.ActiveFrom)
                .FirstOrDefault();
            
            return new GetNextUnreadGlobalFundingRuleResult{ Rule = nextRule };
        }
    }
}
