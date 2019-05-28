using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IFundingRulesService
    {
        Task<GetFundingRulesApiResponse> GetFundingRules();
        Task<GetFundingRulesApiResponse> GetUnreadFundingRules(string id);
        Task<GetAvailableDatesApiResponse> GetAvailableDates();
        Task<GetAccountFundingRulesApiResponse> GetAccountFundingRules(long accountId);
    }
}
