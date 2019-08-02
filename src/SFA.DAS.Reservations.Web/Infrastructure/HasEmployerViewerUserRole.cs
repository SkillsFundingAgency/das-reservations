using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class HasEmployerViewerUserRole : AuthorizationHandler<HasEmployerViewerUserRoleRequirement>
    {
       
        private readonly IEmployerAccountAuthorisationHandler _employerAccountAuthorizationHandler;

        public HasEmployerViewerUserRole(IEmployerAccountAuthorisationHandler employerAccountAuthorizationHandler)
        {
           
            _employerAccountAuthorizationHandler = employerAccountAuthorizationHandler;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasEmployerViewerUserRoleRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext employerContext &&
                employerContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId))
            {
                if (!_employerAccountAuthorizationHandler.IsEmployerAuthorised(context, true))
                {
                    return Task.CompletedTask;
                }
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
