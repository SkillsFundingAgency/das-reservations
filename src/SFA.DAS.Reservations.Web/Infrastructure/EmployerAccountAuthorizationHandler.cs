using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Models.Authentication;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class EmployerAccountAuthorizationHandler : AuthorizationHandler<EmployerAccountRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountRequirement requirement)
        {
            if (!(context.Resource is AuthorizationFilterContext mvcContext) ||
                !mvcContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId)) 
                return Task.CompletedTask;

            if (!context.User.HasClaim(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)))
                return Task.CompletedTask;
            
            var accountIdFromUrl = mvcContext.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();
            var employerAccountClaim = context.User.FindFirst(c=>c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            var employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(employerAccountClaim?.Value);

            if (employerAccountClaim == null || !employerAccounts.ContainsKey(accountIdFromUrl))
                return Task.CompletedTask;

            mvcContext.HttpContext.Items.Add(ContextItemKeys.EmployerIdentifier, employerAccounts.GetValueOrDefault(accountIdFromUrl));
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}