using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public class GetAccountLegalEntitiesForProviderRequest : IGetApiRequest
{
    private readonly long _ukprn;

    public string GetUrl => $"{BaseUrl}provideraccounts/{_ukprn}/legalentities-with-create-cohort";
    public string BaseUrl { get; }

    public GetAccountLegalEntitiesForProviderRequest(string apiBaseUrl, long ukprn)
    {
        _ukprn = ukprn;

        BaseUrl = apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/";
    }
}
