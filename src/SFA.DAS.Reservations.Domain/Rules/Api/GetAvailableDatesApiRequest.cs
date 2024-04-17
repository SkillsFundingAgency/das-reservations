using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetAvailableDatesApiRequest : IGetApiRequest
    {
        public GetAvailableDatesApiRequest(string baseUrl, long accountLegalEntityId)
        {
            BaseUrl = baseUrl;
            AccountLegalEntityId = accountLegalEntityId;
        }
        public string BaseUrl { get; }
        public long AccountLegalEntityId{ get; }
        public string GetUrl => $"{BaseUrl}/rules/available-dates/{AccountLegalEntityId}";
    }
}