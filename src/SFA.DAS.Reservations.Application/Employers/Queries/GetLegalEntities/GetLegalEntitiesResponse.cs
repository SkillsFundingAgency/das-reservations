using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities
{
    public class GetLegalEntitiesResponse
    {
        public IEnumerable<AccountLegalEntity> AccountLegalEntities { get; set; }
    }
}