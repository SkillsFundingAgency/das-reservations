using System.Linq;
using System.Security.Claims;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Web.Extensions;

public static class ClaimsExtensions
{
    public static bool IsEmployer(this ClaimsPrincipal user)
    {
        return user.HasClaim(x => x.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
    }
    
    public static bool ClaimsAreEmpty(this ClaimsPrincipal user)
    {
        return !user.Claims.Any();
    }
}