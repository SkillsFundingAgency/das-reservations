using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.ReservationsApi
{
    public abstract class BaseApiRequest
    {
        protected BaseApiRequest(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        [JsonIgnore]
        public string BaseUrl { get; }
        [JsonIgnore]
        public abstract string CreateUrl { get; }
        [JsonIgnore]
        public abstract string GetUrl { get; }
    }
}