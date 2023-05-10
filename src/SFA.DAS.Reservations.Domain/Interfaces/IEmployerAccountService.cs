using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IEmployerAccountService
    {
        Task<List<Claim>> GetClaim(string userId, string claimType, string email);
        Task<IEnumerable<EmployerAccountUser>> GetAccountUsers(long accountId);
    }
}