using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
    {
        private readonly IEmployerAccountService _employerAccountService;
        private readonly ReservationsWebConfiguration _configuration;

        public EmployerAccountPostAuthenticationClaimsHandler(IOptions<ReservationsWebConfiguration> configuration, IEmployerAccountService employerAccountService)
        {
            _employerAccountService = employerAccountService;
            _configuration = configuration.Value;
        }
        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            string userId;
            var email = string.Empty;
            if (_configuration.UseGovSignIn)
            {
                userId = tokenValidatedContext.Principal.Claims
                    .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                    .Value;
                email = tokenValidatedContext.Principal.Claims
                    .First(c => c.Type.Equals(ClaimTypes.Email))
                    .Value;
            }
            else
            {
                userId = tokenValidatedContext.Principal.Claims
                    .First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))
                    .Value;
            }

            var result = await _employerAccountService.GetClaim(userId, EmployerClaims.AccountsClaimsTypeIdentifier, email);

            return result;
        }
    }
}