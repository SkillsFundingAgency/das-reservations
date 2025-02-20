using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.Reservations.Web.Services;

public class EmptyCustomClaimsService : ICustomClaims
{
    public Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
    {
        return Task.FromResult<IEnumerable<Claim>>([]);
    }
}
