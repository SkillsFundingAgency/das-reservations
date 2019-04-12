using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities
{
    public class GetLegalEntitiesQueryHandler : IRequestHandler<GetLegalEntitiesQuery, GetLegalEntitiesResponse>
    {
        private readonly IAccountApiClient _accountApiClient;

        public GetLegalEntitiesQueryHandler(IAccountApiClient accountApiClient)
        {
            _accountApiClient = accountApiClient;
        }

        public async Task<GetLegalEntitiesResponse> Handle(GetLegalEntitiesQuery request, CancellationToken cancellationToken)
        {
            var legalEntityResources = await _accountApiClient.GetLegalEntitiesConnectedToAccount(request.AccountId);

            var legalEntities = new List<LegalEntityViewModel>();
            foreach (var legalEntityResource in legalEntityResources)
            {
                var legalEntity = await _accountApiClient.GetResource<LegalEntityViewModel>(legalEntityResource.Href);
                legalEntities.Add(legalEntity);
            }
            
            return new GetLegalEntitiesResponse
            {
                LegalEntityViewModels = legalEntities
            };
        }
    }
}