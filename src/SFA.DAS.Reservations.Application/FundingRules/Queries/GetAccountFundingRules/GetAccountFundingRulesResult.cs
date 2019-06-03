using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules
{
    public class GetAccountFundingRulesResult
    {
        public GetAccountFundingRulesApiResponse AccountFundingRules { get; set; }
        public GlobalRuleType? ActiveRule { get; set; }
    }
}
