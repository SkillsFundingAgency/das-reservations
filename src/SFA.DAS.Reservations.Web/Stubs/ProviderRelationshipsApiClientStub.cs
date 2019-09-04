using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.Encoding;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;

namespace SFA.DAS.Reservations.Web.Stubs
{
    public class ProviderRelationshipsApiClientStub : IProviderRelationshipsApiClient
    {
        private readonly IEncodingService _encodingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProviderRelationshipsApiClientStub(IEncodingService encodingService, IHttpContextAccessor httpContextAccessor)
        {
            _encodingService = encodingService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<GetAccountProviderLegalEntitiesWithPermissionResponse> GetAccountProviderLegalEntitiesWithPermission(
            GetAccountProviderLegalEntitiesWithPermissionRequest withPermissionRequest,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var hashedAccountId = _httpContextAccessor.HttpContext.GetRouteValue("employerAccountId").ToString();
            var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);

            var accountLegalEntityPublicHashedId = _httpContextAccessor.HttpContext.GetRouteValue("AccountLegalEntityPublicHashedId").ToString();
            var accountLegalEntityId = _encodingService.Decode(accountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
            
            return Task.FromResult(new GetAccountProviderLegalEntitiesWithPermissionResponse
            {
                AccountProviderLegalEntities = new []
                {
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = accountId,
                        AccountPublicHashedId = hashedAccountId,
                        AccountName = "Test Account",
                        AccountLegalEntityId = accountLegalEntityId,
                        AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
                        AccountLegalEntityName = "Test Legal Entity",
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
