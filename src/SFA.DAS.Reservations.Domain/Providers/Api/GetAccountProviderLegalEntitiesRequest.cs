using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public class GetAccountProviderLegalEntitiesRequest : IGetApiRequest
{
    private readonly long _ukprn;
    
    public string GetUrl => $"{BaseUrl}provideraccounts/{_ukprn}/legalentities";
    public string BaseUrl { get; }

    public GetAccountProviderLegalEntitiesRequest(string apiBaseUrl, long ukprn)
    {
        _ukprn = ukprn;
        
        BaseUrl = apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/";
    }
}