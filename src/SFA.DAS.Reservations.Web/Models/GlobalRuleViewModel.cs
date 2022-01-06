using System;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Models
{
	public class GlobalRuleViewModel
	{
        public GlobalRuleViewModel(GlobalRule activeGlobalRule)
        {
			if (activeGlobalRule == null) return;
            ActiveFrom = activeGlobalRule.ActiveFrom;
			ActiveTo = activeGlobalRule.ActiveTo;
			RuleType = activeGlobalRule.RuleType;
		}

        public DateTime? ActiveFrom { get; set; }
		public DateTime? ActiveTo { get; set; }
		public GlobalRuleType RuleType { get; set; }

		public string ActiveRuleActiveToDateText => ActiveTo?.ToString("MMMM yyyy");
	}
}
