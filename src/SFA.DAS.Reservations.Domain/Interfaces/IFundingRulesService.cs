using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IFundingRulesService
    {
        Task<GetFundingRulesApiResponse> GetFundingRules();
        Task<GetAvailableDatesApiResponse> GetAvailableDates();
        Task<GetAccountFundingRulesApiResponse> GetAccountFundingRules(long accountId);
    }
}
