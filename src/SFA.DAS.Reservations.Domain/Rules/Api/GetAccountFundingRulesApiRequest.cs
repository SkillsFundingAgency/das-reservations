using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetAccountFundingRulesApiRequest : IGetApiRequest
    {
        public GetAccountFundingRulesApiRequest(string baseUrl, long accountId)
        {
            BaseUrl = baseUrl;
            AccountId = accountId;
        }
        public string BaseUrl { get; }
        public long AccountId { get; }
        public string GetUrl => $"{BaseUrl}api/rules/account/{AccountId}";
    }
}