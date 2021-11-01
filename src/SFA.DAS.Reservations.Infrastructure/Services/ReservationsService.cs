using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Infrastructure.Services
{
    public class ReservationsService : IReservationsService
    {
        private readonly HttpClient _client;

        public ReservationsService(HttpClient client)
        {
            _client = client;
        }

        public async Task<GetTransferValidityResponse> GetTransferValidity(long senderId, long receiverId, int? pledgeApplicationId = null)
        {
            var url = pledgeApplicationId.HasValue
                ? $"transfers/validity?senderId={senderId}&receiverId={receiverId}&pledgeApplicationId={pledgeApplicationId}"
                : $"transfers/validity?senderId={senderId}&receiverId={receiverId}";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<GetTransferValidityResponse>(await response.Content.ReadAsStringAsync());
        }
    }
}
