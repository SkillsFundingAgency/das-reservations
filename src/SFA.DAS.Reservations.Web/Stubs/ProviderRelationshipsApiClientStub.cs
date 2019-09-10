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
        private const string ProviderHashedAccountId = "AAAAAA";
        private const long ProviderAccountId = 1 ;
        private const string ProviderAccountLegalEntityPublicHashedId = "AAAAAA";
        private const long ProviderAccountLegalEntityId = 1 ;

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
            var hashedAccountId = _httpContextAccessor.HttpContext.GetRouteValue("employerAccountId")?.ToString();
            long accountId, accountLegalEntityId;

            if (hashedAccountId != null)
            {
                accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
            }
            else
            {
                hashedAccountId = ProviderHashedAccountId;
                accountId = ProviderAccountId;
            }

            var accountLegalEntityPublicHashedId = _httpContextAccessor.HttpContext.GetRouteValue("AccountLegalEntityPublicHashedId")?.ToString();
            
            
            if (accountLegalEntityPublicHashedId != null)
            {
                accountLegalEntityId = _encodingService.Decode(accountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
            }
            else
            {
                accountLegalEntityPublicHashedId = ProviderAccountLegalEntityPublicHashedId;
                accountLegalEntityId = ProviderAccountLegalEntityId;
            }
            
         
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
