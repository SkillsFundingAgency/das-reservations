using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public class ProviderAuthorizationHandler(ILogger<ProviderAuthorizationHandler> logger) : AuthorizationHandler<ProviderUkPrnRequirement>, IProviderAuthorisationHandler
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProviderUkPrnRequirement requirement)
    {
        if (!IsProviderAuthorised(context))
        {
            return Task.CompletedTask;
        }
                
        context.Succeed(requirement);

        return Task.CompletedTask;
    }

    public bool IsProviderAuthorised(AuthorizationHandlerContext context)
    {
        logger.LogInformation("ProviderAuthorizationHandler.IsProviderAuthorised() claims: {claims}",
            JsonConvert.SerializeObject(context.User.Claims.ToDictionary(claim => claim.Type, claim => claim.Value))
        );
            
        if (!(context.Resource is HttpContext providerContext && providerContext.Request.RouteValues.ContainsKey(RouteValues.UkPrn)))
            return false;

        if (!context.User.HasClaim(c => c.Type.Equals(ProviderClaims.ProviderUkprn)))
            return false;

        var ukPrnFromUrl = providerContext.Request.RouteValues[RouteValues.UkPrn].ToString().ToUpper();
        var ukPrn = context.User.FindFirst(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;

        if (ukPrn == ukPrnFromUrl)
        {
            return true;
        }
            
        context.Fail();
                
        return false;
    }
}