using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetUnreadFundingRules
{
    public class GetUnreadFundingRulesResult
    {
        public GetFundingRulesApiResponse FundingRules { get; set; }
    }
}
