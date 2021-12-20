using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Rules
{
    public enum GlobalRuleType
    {
        None = 0,
        FundingPaused = 1,
        ReservationLimit = 2,
        DynamicPause = 3
    }

    public class GlobalRule
    {
        public long Id { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public GlobalRuleType RuleType { get; set; }
        public AccountRestriction Restriction { get; set; }
        public IEnumerable<UserRuleAcknowledgement> UserRuleAcknowledgements { get; set; }

        
        [JsonIgnore]
        public string RuleTypeText => Enum.GetName(typeof(GlobalRuleType), RuleType);
        [JsonIgnore]
        public string RestrictionText => Enum.GetName(typeof(AccountRestriction), Restriction);
    }
}
