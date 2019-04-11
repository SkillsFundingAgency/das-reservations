using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Account.Api.Client;

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
            await _accountApiClient.GetLegalEntitiesConnectedToAccount(request.AccountId);
            
            return null;
        }
    }
}