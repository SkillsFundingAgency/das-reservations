using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.Providers.Services
{
    public interface IProviderService
    {
        Task<AccountLegalEntity> GetAccountLegalEntityById(long id);
        Task<IEnumerable<AccountLegalEntity>> GetTrustedEmployers(uint ukPrn);
    }
}
