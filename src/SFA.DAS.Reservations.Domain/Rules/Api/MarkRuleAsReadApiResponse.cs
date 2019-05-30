using System;
using System.Data;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class MarkRuleAsReadApiResponse
    {
        public long Id { get; set; }
        public long CourseRuleId { get; set; }
        public long GlobalRuleId { get; set; }
        public int UkPrn { get; set; }
        public Guid UserId { get; set; }
        public Rule CourseRule { get; set; }
        public GlobalRule GlobalRule { get; set; }
    }
}
