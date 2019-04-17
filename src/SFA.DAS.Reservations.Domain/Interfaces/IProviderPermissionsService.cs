using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IProviderPermissionsService
    {
        Task<IEnumerable<Employer>> GetTrustedEmployers(uint ukPrn);
    }
}
