using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public record GetAccountProviderLegalEntitiesWithCreateCohortResponse
{
    public IEnumerable<AccountProviderLegalEntityDto> AccountProviderLegalEntities { get; set; }

}

public record AccountProviderLegalEntityDto
{
    public long AccountId { get; set; }
}