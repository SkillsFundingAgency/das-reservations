using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Application.FundingRules.Services
{
    public class RulesService : IRulesService
    {
        private readonly IApiClient _apiClient;
        private readonly IOptions<ReservationsApiConfiguration> _options;

        public RulesService(IApiClient apiClient, IOptions<ReservationsApiConfiguration> options)
        {
            _apiClient = apiClient;
            _options = options;
        }

        public async Task<GetFundingRulesApiResponse> GetFundingRules()
        {
            var request = new GetFundingRulesApiRequest(_options.Value.Url);

            var response = await _apiClient.Get<GetFundingRulesApiResponse>(request);

            return response;
        }
    }
}
