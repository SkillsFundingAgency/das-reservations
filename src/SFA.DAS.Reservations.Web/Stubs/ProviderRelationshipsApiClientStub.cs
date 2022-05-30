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
            const int accountId = 1;
            const int accountLegalEntityId1 = 123;
            const int accountLegalEntityId2 = 456;
            const int accountLegalEntityId3 = 789;

            return Task.FromResult(new GetAccountProviderLegalEntitiesWithPermissionResponse
            {
                AccountProviderLegalEntities = new []
                {
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = accountId,
                        AccountPublicHashedId = _encodingService.Encode(accountId,EncodingType.PublicAccountId),
                        AccountName = "Account 1",
                        AccountLegalEntityId = accountLegalEntityId1,
                        AccountLegalEntityPublicHashedId =_encodingService.Encode(accountLegalEntityId1, EncodingType.PublicAccountLegalEntityId),
                        AccountLegalEntityName = "Legal Entity 1",
                        AccountProviderId = withPermissionRequest.Ukprn
                    },
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = accountId,
                        AccountPublicHashedId = _encodingService.Encode(accountId,EncodingType.PublicAccountId),
                        AccountName = "Account 1",
                        AccountLegalEntityId = accountLegalEntityId2,
                        AccountLegalEntityPublicHashedId = _encodingService.Encode(accountLegalEntityId2, EncodingType.PublicAccountLegalEntityId),
                        AccountLegalEntityName = "Legal Entity 2",
                        AccountProviderId = withPermissionRequest.Ukprn
                    },
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = accountId,
                        AccountPublicHashedId = _encodingService.Encode(accountId,EncodingType.PublicAccountId),
                        AccountName = "Account 1",
                        AccountLegalEntityId = accountLegalEntityId3,
                        AccountLegalEntityPublicHashedId = _encodingService.Encode(accountLegalEntityId3, EncodingType.PublicAccountLegalEntityId),
                        AccountLegalEntityName = "Legal Entity 3",
                        AccountProviderId = withPermissionRequest.Ukprn
                    }
                }
            });
        }

        #region not implemented
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

        public Task Ping(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RevokePermissions(RevokePermissionsRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
