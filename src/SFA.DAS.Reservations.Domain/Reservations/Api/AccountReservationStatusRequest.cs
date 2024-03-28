using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class AccountReservationStatusRequest : IGetApiRequest
    {
        public AccountReservationStatusRequest(string baseUrl, long accountId)
        {
            BaseUrl = baseUrl;
            AccountId = accountId;
        }
        
        public long AccountId { get; }
        public string BaseUrl { get; }
        public string GetUrl => $"{BaseUrl}api/accounts/{AccountId}/status";
    }
}
