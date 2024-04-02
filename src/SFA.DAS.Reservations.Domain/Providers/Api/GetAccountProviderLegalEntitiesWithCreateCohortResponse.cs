using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public record GetAccountProviderLegalEntitiesWithCreateCohortResponse
{
    public IEnumerable<GetProviderAccountLegalEntityWithCreatCohortItem> ProviderAccountLegalEntities { get; set; }

}

public record GetProviderAccountLegalEntityWithCreatCohortItem
{
    public long AccountId { get; set; }
}