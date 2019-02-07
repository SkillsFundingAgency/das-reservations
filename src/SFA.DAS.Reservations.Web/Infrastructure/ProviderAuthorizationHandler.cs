using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class ProviderAuthorizationHandler : AuthorizationHandler<ProviderUkPrnRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProviderUkPrnRequirement requirement)
        {
            if (!(context.Resource is AuthorizationFilterContext mvcContext) || !mvcContext.RouteData.Values.ContainsKey(RouteValues.UkPrn))
                return Task.CompletedTask;

            if (!context.User.HasClaim(c => c.Type.Equals(ProviderClaims.ProviderUkprn)))
                return Task.CompletedTask;

            var ukPrnFromUrl = mvcContext.RouteData.Values[RouteValues.UkPrn].ToString().ToUpper();
            var ukPrn = context.User.FindFirst(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;

            if (ukPrn != ukPrnFromUrl)
            {
                context.Fail();
                
                return Task.CompletedTask;
            }
                
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}