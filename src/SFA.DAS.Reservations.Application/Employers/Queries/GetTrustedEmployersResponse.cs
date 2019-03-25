using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.Employers.Queries
{
    public class GetTrustedEmployersResponse
    {
        public IEnumerable<Employer> Employers { get; set; }
    }
}
