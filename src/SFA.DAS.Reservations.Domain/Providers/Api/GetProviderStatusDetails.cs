using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Providers.Api
{
    public class GetProviderStatusDetails : IGetApiRequest
    {
        private readonly long _ukprn;
        public string BaseUrl { get; }
        public string GetUrl => $"{BaseUrl}api/provideraccounts/{_ukprn}";

        public GetProviderStatusDetails(string baseUrl, long ukprn)
        {
            _ukprn = ukprn;
            BaseUrl = baseUrl;
        }
    }
}
