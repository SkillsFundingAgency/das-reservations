﻿using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public enum RuleType
    {
        None = 0,
        CourseRule = 1,
        GlobalRule = 2
    }

    public class MarkRuleAsReadApiRequest : IPostApiRequest
    {
        public string Id { get; set; }
        public long RuleId { get; set; }
        public RuleType TypeOfRule {get;set;}

        public string BaseUrl { get; }
        public string CreateUrl => $"{BaseUrl}api/rules/upcoming";

        public MarkRuleAsReadApiRequest(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
    }
}
