using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Reservations.Models.Authentication;

namespace SFA.DAS.Reservations.Web.Services
{
    public interface IEmployerAccountService
    {
        Task<IEnumerable<EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId);
        EmployerIdentifier GetCurrentEmployerAccountId(HttpContext routeData);
        Task<IEnumerable<EmployerIdentifier>> GetUserRoles(IEnumerable<EmployerIdentifier> values, string userId);
        Task<Claim> GetClaim(string userId);
    }
}