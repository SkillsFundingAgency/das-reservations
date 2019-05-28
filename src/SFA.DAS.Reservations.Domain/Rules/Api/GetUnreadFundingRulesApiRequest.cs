using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetUnreadFundingRulesApiRequest : IGetApiRequest
    {
        public GetUnreadFundingRulesApiRequest(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public string Id { get; set; }
        public string BaseUrl { get; }
        public string GetUrl => $"{BaseUrl}api/rules/upcoming/{Id}";
    }
}
