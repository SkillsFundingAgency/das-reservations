using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules
{
    public class GetFundingRulesResult
    {
        public ICollection<ReservationRule> AccountRules { get; set; }
        public ICollection<GlobalRule> GlobalRules { get; set; }

        public IEnumerable<ReservationRule> ActiveAccountRules => AccountRules?.Where(rule =>
            rule.ActiveFrom <= DateTime.Now && rule.ActiveTo >= DateTime.Now) ?? new List<ReservationRule>();
        public IEnumerable<GlobalRule> ActiveGlobalRules  => GlobalRules?.Where(rule => rule.ActiveFrom <= DateTime.Now) ?? new List<GlobalRule>();
    }
}
