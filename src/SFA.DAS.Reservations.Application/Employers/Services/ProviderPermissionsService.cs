using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Reservations.Domain.Employers;
using StructureMap.Query;

namespace SFA.DAS.Reservations.Application.Employers.Services
{
    public class ProviderPermissionsService : IProviderPermissionsService
    {
        private readonly IProviderRelationshipsApiClient _apiClient;

        public ProviderPermissionsService(IProviderRelationshipsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<Employer>> GetTrustedEmployers(uint ukPrn)
        {
            if (ukPrn == default(uint))
            {
                throw new ArgumentException("Ukprn must be set to a non default value", nameof(ukPrn));
            }

            var trustedEmployers = await _apiClient.GetAccountProviderLegalEntitiesWithPermission(
                new GetAccountProviderLegalEntitiesWithPermissionRequest
                {
                    Operation = Operation.CreateCohort,
                    Ukprn = ukPrn
                });

            return trustedEmployers?.AccountProviderLegalEntities?.Select(e => new Employer
            {
                AccountId = e.AccountId,
                AccountPublicHashedId = e.AccountPublicHashedId,
                AccountName = e.AccountName,
                AccountLegalEntityId = e.AccountLegalEntityId,
                AccountLegalEntityPublicHashedId = e.AccountLegalEntityPublicHashedId,
                AccountLegalEntityName = e.AccountLegalEntityName
            }).ToArray();

        }
    }
}
