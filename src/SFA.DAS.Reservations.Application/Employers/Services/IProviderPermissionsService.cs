using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.Employers.Services
{
    public interface IProviderPermissionsService
    {
        Task<IEnumerable<Employer>> GetTrustedEmployers(long ukPrn);
    }
}
