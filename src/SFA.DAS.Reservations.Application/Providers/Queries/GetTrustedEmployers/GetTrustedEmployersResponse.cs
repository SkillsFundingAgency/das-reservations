using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers
{
    public class GetTrustedEmployersResponse
    {
        public IEnumerable<AccountLegalEntity> Employers { get; set; }
    }
}
