using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public record GetAccountProviderLegalEntitiesWithCreateCohortResponse
{
    public List<GetProviderAccountLegalEntityWithCreatCohortItem> AccountProviderLegalEntities { get; set; }

}

public record GetProviderAccountLegalEntityWithCreatCohortItem
{
    public long AccountId { get; set; }
}