using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api
{
    public class GetTransferValidityRequest : IGetApiRequest
    {
        private long _senderId;
        private long _receiverId;
        private int? _pledgeApplicationId;

        public GetTransferValidityRequest(string apiBaseUrl, long senderId, long receiverId, int? pledgeApplicationId)
        {
            BaseUrl = apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/";
            _senderId = senderId;
            _receiverId = receiverId;
            _pledgeApplicationId = pledgeApplicationId;
        }

        public string GetUrl => $"{BaseUrl}transfers/validity?senderId={_senderId}&receiverId={_receiverId}{(_pledgeApplicationId.HasValue ? $"&pledgeApplicationId={_pledgeApplicationId}" : string.Empty)}";

        public string BaseUrl { get; }
    }
}
