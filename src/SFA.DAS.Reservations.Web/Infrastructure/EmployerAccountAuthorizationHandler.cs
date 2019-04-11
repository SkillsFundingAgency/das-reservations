using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Models.Authentication;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class EmployerAccountAuthorizationHandler : AuthorizationHandler<EmployerAccountRequirement>, IEmployerAccountAuthorisationHandler
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountRequirement requirement)
        {
            if (!IsEmployerAuthorised(context))
            {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }

        public bool IsEmployerAuthorised(AuthorizationHandlerContext context)
        {
            if (!(context.Resource is AuthorizationFilterContext mvcContext) || !mvcContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId)) 
                return false;

            if (!context.User.HasClaim(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)))
                return false;
            
            var accountIdFromUrl = mvcContext.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();
            var employerAccountClaim = context.User.FindFirst(c=>c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            var employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(employerAccountClaim?.Value);

            if (employerAccountClaim == null || !employerAccounts.ContainsKey(accountIdFromUrl))
                return false;

            mvcContext.HttpContext.Items.Add(ContextItemKeys.EmployerIdentifier, employerAccounts.GetValueOrDefault(accountIdFromUrl));

            return true;
        }
    }
}