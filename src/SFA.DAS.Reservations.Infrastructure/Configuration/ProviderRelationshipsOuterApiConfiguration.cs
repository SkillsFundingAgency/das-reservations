using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Configuration;
public class ProviderRelationshipsOuterApiConfiguration : IApimClientConfiguration
{
    public string ApiBaseUrl { get; set; }
    public string SubscriptionKey { get; set; }
    public string ApiVersion { get; set; }
}
