using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.ProviderRelationships;
using SFA.DAS.Reservations.Domain.ProviderRelationships.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Services
{
    public class ProviderPermissionsService(IProviderRelationshipsOuterApiClient apiClient, IOptions<ProviderRelationshipsOuterApiConfiguration> options)
        : IProviderPermissionsService
    {
        private readonly ProviderRelationshipsOuterApiConfiguration _config = options.Value;

        public async Task<IEnumerable<Employer>> GetTrustedEmployers(uint ukPrn)
        {
            if (ukPrn == default(uint))
            {
                throw new ArgumentException("Ukprn must be set to a non default value", nameof(ukPrn));
            }

            List<Operation> operations = new List<Operation> { Operation.CreateCohort };

            var request =
                new GetAccountProviderLegalEntitiesWithPermissionRequest(_config.ApiBaseUrl, operations, (int)ukPrn);

            var trustedEmployers = await apiClient.Get<GetAccountProviderLegalEntitiesWithPermissionResponse>(request);

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
