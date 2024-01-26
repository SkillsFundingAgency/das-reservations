using System.Collections.Generic;
using System.Security.Claims;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Web.Services
{
    public interface IUserClaimsService
    {
        bool UserIsInRole(string employerAccountId, EmployerUserRole userRole, IEnumerable<Claim> claims);
    }
}