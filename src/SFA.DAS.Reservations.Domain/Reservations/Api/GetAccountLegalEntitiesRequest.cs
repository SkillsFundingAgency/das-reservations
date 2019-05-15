using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class GetAccountLegalEntitiesRequest : IGetAllApiRequest
    {
        public string AccountId { get; }
        public string BaseUrl { get; }
        public string GetAllUrl => $"{BaseUrl}api/accountlegalentities/{AccountId}";

        public GetAccountLegalEntitiesRequest(string baseUrl, string accountId)
        {
            AccountId = accountId;
            BaseUrl = baseUrl;
        }
    }
}
