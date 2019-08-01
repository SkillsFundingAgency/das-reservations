using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities
{
    public class GetLegalEntitiesQueryHandler : IRequestHandler<GetLegalEntitiesQuery, GetLegalEntitiesResponse>
    {
        private readonly IApiClient _apiClient;
        private readonly IEncodingService _encodingService;
        private readonly ReservationsApiConfiguration _configuration;

        public GetLegalEntitiesQueryHandler(
            IApiClient apiClient,
            IOptions<ReservationsApiConfiguration> options,
            IEncodingService encodingService)
        {
            _apiClient = apiClient;
            _encodingService = encodingService;
            _configuration = options.Value;
        }

        public async Task<GetLegalEntitiesResponse> Handle(GetLegalEntitiesQuery request, CancellationToken cancellationToken)
        {
            var legalEntities = await _apiClient.GetAll<AccountLegalEntity>(new GetAccountLegalEntitiesRequest(_configuration.Url, request.AccountId));

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

            return new GetLegalEntitiesResponse
            {
                AccountLegalEntities = accountLegalEntities
            };
        }
    }
}