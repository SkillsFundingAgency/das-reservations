﻿using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;

namespace SFA.DAS.Reservations.Web.Stubs
{
    public class ProviderRelationshipsApiClientStub : IProviderRelationshipsApiClient
    {
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
                        AccountPublicHashedId = "111ABC",
                        AccountName = "Account 1",
                        AccountLegalEntityId = 11,
                        AccountLegalEntityPublicHashedId = "111DEF",
                        AccountLegalEntityName = "Legal Entity 1",
                        AccountProviderId = withPermissionRequest.Ukprn
                    },
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = 1,
                        AccountPublicHashedId = "111ABC",
                        AccountName = "Account 1",
                        AccountLegalEntityId = 22,
                        AccountLegalEntityPublicHashedId = "222DEF",
                        AccountLegalEntityName = "Legal Entity 2",
                        AccountProviderId = withPermissionRequest.Ukprn
                    },
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = 1,
                        AccountPublicHashedId = "111ABC",
                        AccountName = "Account 1",
                        AccountLegalEntityId = 33,
                        AccountLegalEntityPublicHashedId = "333DEF",
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