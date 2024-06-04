using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Services;

public class ReservationsOuterService(IReservationsOuterApiClient apiClient, IOptions<ReservationsOuterApiConfiguration> options)
    : IReservationsOuterService
{
    private readonly ReservationsOuterApiConfiguration _config = options.Value;

    public async Task<GetTransferValidityResponse> GetTransferValidity(long senderId, long receiverId, int? pledgeApplicationId = null)
    {
        var request = new GetTransferValidityRequest(_config.ApiBaseUrl, senderId, receiverId, pledgeApplicationId);

        return await apiClient.Get<GetTransferValidityResponse>(request);
    }

    public async Task<ProviderAccountResponse> GetProviderStatus(long ukprn)
    {
        return await apiClient.Get<ProviderAccountResponse>(new GetProviderStatusDetails(_config.ApiBaseUrl, ukprn));
    }

    public async Task<GetAccountLegalEntitiesForProviderResponse> GetAccountProviderLegalEntitiesWithCreateCohort(long ukprn)
    {
        var request = new GetAccountLegalEntitiesForProviderRequest(_config.ApiBaseUrl, ukprn);

        return await apiClient.Get<GetAccountLegalEntitiesForProviderResponse>(request);
    }

    public async Task<bool> CanAccessCohort(long partyId, long cohortId)
    {
        var content = new GetCohortAccessRequest(_config.ApiBaseUrl, partyId, cohortId);

        var response = await apiClient.Get<GetCohortAccessResponse>(content);

        return response.HasCohortAccess;
    }

    public async Task<GetAvailableDatesApiResponse> GetAvailableDates(long accountLegalEntityId)
    {
        return await apiClient.Get<GetAvailableDatesApiResponse>(new GetAvailableDatesApiRequest(_config.ApiBaseUrl, accountLegalEntityId));
    }

    public async Task<IEnumerable<Employer>> GetTrustedEmployers(uint ukPrn)
    {
        if (ukPrn == default(uint))
        {
            throw new ArgumentException("Ukprn must be set to a non default value", nameof(ukPrn));
        }

        List<Operation> operations = new List<Operation> { Operation.CreateCohort };

        var request =
            new GetAccountProviderLegalEntitiesWithPermissionRequest(_config.ApiBaseUrl, operations, (int)ukPrn);

        var trustedEmployers = GetTrustedEmployers(request).Result;

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

    private async Task<GetAccountProviderLegalEntitiesWithPermissionResponse> GetTrustedEmployers(
        GetAccountProviderLegalEntitiesWithPermissionRequest request)
    {
        return await apiClient.Get<GetAccountProviderLegalEntitiesWithPermissionResponse>(request);
    }
}