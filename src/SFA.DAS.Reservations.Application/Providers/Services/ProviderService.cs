using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.Providers.Services
{
    public class ProviderService : IProviderService
    {
        public Task<AccountLegalEntity> GetAccountLegalEntityById(long id)
        {
            throw new NotImplementedException();
        }
    }
}
