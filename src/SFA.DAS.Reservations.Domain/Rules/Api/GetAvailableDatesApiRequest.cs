using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetAvailableDatesApiRequest : IGetApiRequest
    {
        public GetAvailableDatesApiRequest(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
        public string BaseUrl { get; }
        public string GetUrl => $"{BaseUrl}api/rules/available-dates";
    }
}