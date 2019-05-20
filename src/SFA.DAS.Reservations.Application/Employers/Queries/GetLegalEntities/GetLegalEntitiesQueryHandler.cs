﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities
{
    public class GetLegalEntitiesQueryHandler : IRequestHandler<GetLegalEntitiesQuery, GetLegalEntitiesResponse>
    {
        private readonly IApiClient _apiClient;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly IEncodingService _encodingService;
        private readonly ReservationsApiConfiguration _configuration;

        public GetLegalEntitiesQueryHandler(
            IApiClient apiClient, 
            ICacheStorageService cacheStorageService,
            IOptions<ReservationsApiConfiguration> options,
            IEncodingService encodingService)
        {
            _apiClient = apiClient;
            _cacheStorageService = cacheStorageService;
            _encodingService = encodingService;
            _configuration = options.Value;
        }

        public async Task<GetLegalEntitiesResponse> Handle(GetLegalEntitiesQuery request, CancellationToken cancellationToken)
        {
            var legalEntities = await _cacheStorageService.RetrieveFromCache<IEnumerable<AccountLegalEntity>>(request.AccountId.ToString());

            if (legalEntities != null)
            {
                return new GetLegalEntitiesResponse
                {
                    AccountLegalEntities = legalEntities
                };
            }

            legalEntities = await _apiClient.GetAll<AccountLegalEntity>(new GetAccountLegalEntitiesRequest(_configuration.Url, request.AccountId));

            if (legalEntities != null)
            {
                foreach (var accountLegalEntity in legalEntities)
                {
                    accountLegalEntity.AccountLegalEntityPublicHashedId =
                        _encodingService.Encode(accountLegalEntity.AccountLegalEntityId,
                            EncodingType.PublicAccountLegalEntityId);
                }
            }
            


            var accountLegalEntities = legalEntities as AccountLegalEntity[] ?? legalEntities?.ToArray() ?? new AccountLegalEntity[0];

            if (accountLegalEntities.Any())
            {
                await _cacheStorageService.SaveToCache(request.AccountId.ToString(), accountLegalEntities, 1);
            }

            return new GetLegalEntitiesResponse
            {
                AccountLegalEntities = accountLegalEntities
            };
        }
    }
}