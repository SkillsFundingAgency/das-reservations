using MediatR;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule
{
    public class GetNextUnreadGlobalFundingRuleQuery : IRequest<GetNextUnreadGlobalFundingRuleResult>
    {
        public string Id { get; set; }
    }
}
