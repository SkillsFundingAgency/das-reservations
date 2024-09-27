using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization
{
    public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
    {
        private readonly IEmployerAccountService _employerAccountService;

        public EmployerAccountPostAuthenticationClaimsHandler(IEmployerAccountService employerAccountService)
        {
            _employerAccountService = employerAccountService;
        }
        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            var userId = tokenValidatedContext.Principal.Claims
                .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                .Value;
            var email = tokenValidatedContext.Principal.Claims
                    .First(c => c.Type.Equals(ClaimTypes.Email))
                    .Value;
            

            var result = await _employerAccountService.GetClaim(userId, EmployerClaims.AccountsClaimsTypeIdentifier, email);

            return result;
        }
    }
}