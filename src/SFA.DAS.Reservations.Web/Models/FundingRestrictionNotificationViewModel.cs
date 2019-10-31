using System;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Web.Models
{
    public class FundingRestrictionNotificationViewModel
    {
        public long RuleId { get; set; }
        public RuleType TypeOfRule { get; set; }
        public DateTime RestrictionStartDate { get; set; }
        public string BackLink { get; set; }
        public string RouteName { get; set; }
    }
}
