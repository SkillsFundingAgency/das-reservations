﻿using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.FundingRules.Services
{
    public class FundingRulesService : IFundingRulesService
    {
        private readonly IApiClient _apiClient;
        private readonly IOptions<ReservationsApiConfiguration> _options;

        public FundingRulesService(IApiClient apiClient, IOptions<ReservationsApiConfiguration> options)
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

        public async Task<GetAvailableDatesApiResponse> GetAvailableDates()
        {
            var request = new GetAvailableDatesApiRequest(_options.Value.Url);

            var response = await _apiClient.Get<GetAvailableDatesApiResponse>(request);

            return response;
        }
    }
}