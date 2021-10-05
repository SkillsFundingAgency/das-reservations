using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Configuration
{
    public class ReservationsOuterApiConfiguration : IApimClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string SubscriptionKey { get; set; }
        public string ApiVersion { get; set; }
    }
}
