using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules
{
    public class GetAccountFundingRulesResult
    {
        public GetAccountFundingRulesApiResponse AccountFundingRules { get; set; }
        public GlobalRule ActiveRule { get; set; }
    }
}
