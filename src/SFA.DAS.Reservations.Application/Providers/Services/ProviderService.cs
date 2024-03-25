using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Providers.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IApiClient _apiClient;
        private readonly ReservationsApiConfiguration _configuration;

        public ProviderService(IApiClient apiClient, IOptions<ReservationsApiConfiguration> options)
        {
            _apiClient = apiClient;
            _configuration = options.Value;
        }

        public async Task<AccountLegalEntity> GetAccountLegalEntityById(long id)
        {
            var request = new GetAccountLegalEntityRequest(_configuration.Url, id);

            var legalEntity = await _apiClient.Get<AccountLegalEntity>(request);

            return legalEntity;
        }

        public async Task<IEnumerable<AccountLegalEntity>> GetTrustedEmployers(uint ukPrn)
        {
            var request = new GetTrustedEmployersRequest(_configuration.Url, ukPrn);

            var legalEntities = await _apiClient.GetAll<AccountLegalEntity>(request);

            return legalEntities;

        }
    }
}
