using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntity
{
    public class GetLegalEntityQueryHandler : IRequestHandler<GetLegalEntityQuery, GetLegalEntityResponse>
    {
        private readonly IApiClient _apiClient;
        private readonly IEncodingService _encodingService;
        private readonly ReservationsApiConfiguration _configuration;

        public GetLegalEntityQueryHandler(
            IApiClient apiClient,
            IOptions<ReservationsApiConfiguration> options,
            IEncodingService encodingService)
        {
            _apiClient = apiClient;
            _encodingService = encodingService;
            _configuration = options.Value;
        }

        public async Task<GetLegalEntityResponse> Handle(GetLegalEntityQuery request, CancellationToken cancellationToken)
        {
            var legalEntity = await _apiClient.Get<AccountLegalEntity>(new GetAccountLegalEntityRequest(_configuration.Url, request.Id));

            if (legalEntity != null)
            {
                legalEntity.AccountLegalEntityPublicHashedId = _encodingService.Encode(legalEntity.AccountLegalEntityId, EncodingType.PublicAccountLegalEntityId);
            }

            return new GetLegalEntityResponse
            {
               AccountLegalEntity = legalEntity
            };
        }
    }
}