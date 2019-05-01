using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Rules
{
    public enum GlobalRuleType
    {
        FundingPaused = 0,
        ReservationLimit = 1
    }

    public class GlobalRule
    {
        public long Id { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public GlobalRuleType RuleType { get; set; }
        public AccountRestriction Restriction { get; set; }
        
        [JsonIgnore]
        public string RuleTypeText => Enum.GetName(typeof(GlobalRuleType), RuleType);
        [JsonIgnore]
        public string RestrictionText => Enum.GetName(typeof(AccountRestriction), Restriction);
    }
}
