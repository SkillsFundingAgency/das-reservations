using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Encoding;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;

namespace SFA.DAS.Reservations.Web.Stubs
{
    public class ProviderRelationshipsApiClientStub : IProviderRelationshipsApiClient
    {
        private readonly IEncodingService _encodingService;

        public ProviderRelationshipsApiClientStub(IEncodingService encodingService)
        {
            _encodingService = encodingService;
        }

        public Task<GetAccountProviderLegalEntitiesWithPermissionResponse> GetAccountProviderLegalEntitiesWithPermission(
            GetAccountProviderLegalEntitiesWithPermissionRequest withPermissionRequest,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(new GetAccountProviderLegalEntitiesWithPermissionResponse
            {
                AccountProviderLegalEntities = new []
                {
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = 1,
                        AccountPublicHashedId = _encodingService.Encode(1,EncodingType.PublicAccountId),
                        AccountName = "Account 1",
                        AccountLegalEntityId = 123,
                        AccountLegalEntityPublicHashedId =_encodingService.Encode(123, EncodingType.PublicAccountLegalEntityId),
                        AccountLegalEntityName = "Legal Entity 1",
                        AccountProviderId = withPermissionRequest.Ukprn
                    },
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = 1,
                        AccountPublicHashedId = _encodingService.Encode(1,EncodingType.PublicAccountId),
                        AccountName = "Account 1",
                        AccountLegalEntityId = 456,
                        AccountLegalEntityPublicHashedId = _encodingService.Encode(456, EncodingType.PublicAccountLegalEntityId),
                        AccountLegalEntityName = "Legal Entity 2",
                        AccountProviderId = withPermissionRequest.Ukprn
                    },
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = 1,
                        AccountPublicHashedId = _encodingService.Encode(1,EncodingType.PublicAccountId),
                        AccountName = "Account 1",
                        AccountLegalEntityId = 789,
                        AccountLegalEntityPublicHashedId = _encodingService.Encode(789, EncodingType.PublicAccountLegalEntityId),
                        AccountLegalEntityName = "Legal Entity 3",
                        AccountProviderId = withPermissionRequest.Ukprn
                    }
                }
            });
        }

        public Task<bool> HasPermission(HasPermissionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasRelationshipWithPermission(HasRelationshipWithPermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task HealthCheck()
        {
            throw new NotImplementedException();
        }
    }
}
