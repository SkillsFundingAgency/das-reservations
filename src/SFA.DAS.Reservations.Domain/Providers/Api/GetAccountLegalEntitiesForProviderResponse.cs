using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public record GetAccountLegalEntitiesForProviderResponse
{
    public List<GetAccountLegalEntitiesForProviderItem> AccountProviderLegalEntities { get; set; }
}

public record GetAccountLegalEntitiesForProviderItem
{
    public long AccountId { get; set; }
    public long AccountLegalEntityId { get; set; }
}