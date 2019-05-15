using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities
{
    public class GetLegalEntitiesQueryHandler : IRequestHandler<GetLegalEntitiesQuery, GetLegalEntitiesResponse>
    {
        private readonly IApiClient _accountApiClient;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly ReservationsApiConfiguration _configuration;

        public GetLegalEntitiesQueryHandler(
            IApiClient apiClient, 
            ICacheStorageService cacheStorageService,
            IOptions<ReservationsApiConfiguration> options)
        {
            _accountApiClient = apiClient;
            _cacheStorageService = cacheStorageService;
            _configuration = options.Value;
        }

        public async Task<GetLegalEntitiesResponse> Handle(GetLegalEntitiesQuery request, CancellationToken cancellationToken)
        {
            var legalEntities = await _cacheStorageService.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(request.AccountId);

            if (legalEntities != null)
            {
                return new GetLegalEntitiesResponse
                {
                    AccountLegalEntities = legalEntities
                };
            }

            legalEntities = await _accountApiClient.GetAll<AccountLegalEntity>(new GetAccountLegalEntitiesRequest(_configuration.Url, request.AccountId));

            var accountLegalEntities = legalEntities as AccountLegalEntity[] ?? legalEntities?.ToArray() ?? new AccountLegalEntity[0];

            if (accountLegalEntities.Any())
            {
                await _cacheStorageService.SaveToCache(request.AccountId, accountLegalEntities, 1);
            }

            return new GetLegalEntitiesResponse
            {
                AccountLegalEntities = accountLegalEntities
            };
        }
    }
}