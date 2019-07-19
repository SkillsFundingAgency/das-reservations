using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class GetAccountLegalEntitiesRequest : IGetAllApiRequest
    {
        public long AccountId { get; }
        public string BaseUrl { get; }
        public string GetAllUrl => $"{BaseUrl}api/{AccountId}/accountlegalentities";

        public GetAccountLegalEntitiesRequest(string baseUrl, long accountId)
        {
            AccountId = accountId;
            BaseUrl = baseUrl;
        }
    }
}
