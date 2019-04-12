using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities
{
    public class GetLegalEntitiesQueryHandler : IRequestHandler<GetLegalEntitiesQuery, GetLegalEntitiesResponse>
    {
        private readonly IAccountApiClient _accountApiClient;
        private readonly ICacheStorageService _cacheStorageService;

        public GetLegalEntitiesQueryHandler(
            IAccountApiClient accountApiClient, 
            ICacheStorageService cacheStorageService)
        {
            _accountApiClient = accountApiClient;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<GetLegalEntitiesResponse> Handle(GetLegalEntitiesQuery request, CancellationToken cancellationToken)
        {
            var legalEntities = await _cacheStorageService.RetrieveFromCache<IEnumerable<LegalEntityViewModel>>(request.AccountId);

            if (legalEntities != null)
            {
                return new GetLegalEntitiesResponse
                {
                    LegalEntityViewModels = legalEntities
                };
            }
            
            var legalEntityResources = await _accountApiClient.GetLegalEntitiesConnectedToAccount(request.AccountId);

            legalEntities = new List<LegalEntityViewModel>();
            foreach (var legalEntityResource in legalEntityResources)
            {
                var legalEntity = await _accountApiClient.GetResource<LegalEntityViewModel>(legalEntityResource.Href);
                ((List<LegalEntityViewModel>)legalEntities).Add(legalEntity);
            }

            await _cacheStorageService.SaveToCache(request.AccountId, legalEntities, 1);

            return new GetLegalEntitiesResponse
            {
                LegalEntityViewModels = legalEntities
            };
        }
    }
}