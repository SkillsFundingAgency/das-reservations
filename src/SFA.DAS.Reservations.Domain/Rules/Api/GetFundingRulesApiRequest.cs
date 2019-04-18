using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetFundingRulesApiRequest : IGetApiRequest
    {
        public GetFundingRulesApiRequest(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
        public string BaseUrl { get; }
        public string GetUrl => $"{BaseUrl}api/rules";
    }
}
