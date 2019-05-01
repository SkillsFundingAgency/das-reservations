using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetAccountFundingRulesApiResponse
    {
        public IList<GlobalRule> GlobalRules { get; set; }
    }
}