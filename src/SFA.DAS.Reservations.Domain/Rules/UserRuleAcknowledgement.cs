using System;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Domain.Rules
{
    public class UserRuleAcknowledgement
    {
        public long UserRuleNotificationId { get;  set;}
        public string Id { get; set; }
        public long RuleId { get; set; }
        public RuleType TypeOfRule { get; set; }
        public Guid? UserId { get; set;}
        public int? UkPrn { get; set;}
        public long? CourseRuleId { get; set; }
        public long? GlobalRuleId { get; set;}
    }
}
