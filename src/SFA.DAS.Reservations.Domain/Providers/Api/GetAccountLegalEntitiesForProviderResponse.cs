using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public record GetAccountLegalEntitiesForProviderResponse
{
    public List<GetAccountLegalEntitiesForProviderItem> AccountProviderLegalEntities { get; set; }
}

public record GetAccountLegalEntitiesForProviderItem
{
    public long AccountId { get; set; }

    public string AccountHashedId { get; set; }

    public string AccountPublicHashedId { get; set; }

    public string AccountName { get; set; }

    public long AccountLegalEntityId { get; set; }

    public string AccountLegalEntityPublicHashedId { get; set; }

    public string AccountLegalEntityName { get; set; }

    public long AccountProviderId { get; set; }
}
