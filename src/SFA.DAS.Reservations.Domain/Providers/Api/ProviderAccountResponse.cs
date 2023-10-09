using System.Text.Json.Serialization;

namespace SFA.DAS.Reservations.Domain.Providers.Api
{
    public class ProviderAccountResponse
    {
        [JsonPropertyName("canAccessService")]
        public bool CanAccessService { get; set; }
    }
}
