using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Reservations.Api;

public class GetAccountProviderLegalEntitiesWithPermissionResponse
{
    public IEnumerable<AccountProviderLegalEntityDto> AccountProviderLegalEntities { get; set; }
    
    public class AccountProviderLegalEntityDto
    {
        public long Id { get; set; }
        public string ProviderName { get; set; }
        public string AccountLegalEntityName { get; set; }
    }
}