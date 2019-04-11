using System.Collections.Generic;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities
{
    public class GetLegalEntitiesResponse
    {
        public IEnumerable<LegalEntityViewModel> LegalEntityViewModels { get; set; }
    }
}