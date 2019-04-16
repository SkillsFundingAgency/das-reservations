using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Rules.Api;

namespace SFA.DAS.Reservations.Application.FundingRules.Services
{
    public interface IFundingRulesService
    {
        Task<GetFundingRulesApiResponse> GetFundingRules();
    }
}
