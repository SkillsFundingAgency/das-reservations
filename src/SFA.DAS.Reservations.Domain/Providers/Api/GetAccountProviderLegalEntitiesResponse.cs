using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public class GetAccountProviderLegalEntitiesResponse
{
    public IEnumerable<AccountProviderLegalEntityDto> AccountProviderLegalEntities { get; set; }

    public class AccountProviderLegalEntityDto
    {
        public long AccountId { get; set; }
    }
}