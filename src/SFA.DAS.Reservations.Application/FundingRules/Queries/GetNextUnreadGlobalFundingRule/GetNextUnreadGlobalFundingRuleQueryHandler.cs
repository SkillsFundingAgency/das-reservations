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
            var response = await _service.GetFundingRules();

            var globalRules =
                response?.GlobalRules?.Where(rule => rule.ActiveFrom.HasValue && rule.ActiveFrom >= DateTime.Now);

           
            if (Guid.TryParse(request.Id, out var userId))
            {
                globalRules = globalRules?.Where(rule => rule.UserRuleAcknowledgements == null || 
                                                         !rule.UserRuleAcknowledgements.Any() || 
                                                         rule.UserRuleAcknowledgements.Where(a => a.UserId.HasValue).All(a => !a.UserId.Value.Equals(userId)));
            }
            else if (int.TryParse(request.Id, out var ukPrn))
            {
                globalRules = globalRules?.Where(rule =>  rule.UserRuleAcknowledgements == null || 
                                                          !rule.UserRuleAcknowledgements.Any() || 
                                                          rule.UserRuleAcknowledgements.Where(a => a.UkPrn.HasValue).All(a => !a.UkPrn.Value.Equals(ukPrn)));
            }

            var nextRule = globalRules?.OrderBy(r => r.ActiveFrom).FirstOrDefault();
            
            return new GetNextUnreadGlobalFundingRuleResult{ Rule = nextRule };
        }
    }
}
