using MediatR;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead
{
    public class MarkRuleAsReadCommand : IRequest<Unit>
    {
        public string Id { get; set; }
        public long RuleId { get; set; }
        public RuleType TypeOfRule {get;set;}
    }
}
