﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Services
{
    public class ProviderPermissionsService : IProviderPermissionsService
    {
        private readonly IReservationsOuterService _apiClient;

        public ProviderPermissionsService(IReservationsOuterService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<Employer>> GetTrustedEmployers(uint ukPrn)
        {
            if (ukPrn == default(uint))
            {
                throw new ArgumentException("Ukprn must be set to a non default value", nameof(ukPrn));
            }

            var trustedEmployers = await _apiClient.GetAccountProviderLegalEntitiesWithCreateCohort(ukPrn);


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
