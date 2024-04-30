using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IFundingRulesService
    {
        Task<GetFundingRulesApiResponse> GetFundingRules();
        Task<GetAccountFundingRulesApiResponse> GetAccountFundingRules(long accountId);
        Task MarkRuleAsRead(string id, long ruleId, RuleType type);
    }
}
