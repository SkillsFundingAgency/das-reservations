using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class HasProviderOrEmployerAccountAuthorisationHandler  : AuthorizationHandler<HasProviderOrEmployerAccountRequirement>
    {
        private readonly IProviderAuthorisationHandler _providerAuthorizationHandler;
        private readonly IEmployerAccountAuthorisationHandler _employerAccountAuthorizationHandler;

        public HasProviderOrEmployerAccountAuthorisationHandler(
            IProviderAuthorisationHandler providerAuthorizationHandler,
            IEmployerAccountAuthorisationHandler employerAccountAuthorizationHandler)
        {
            _providerAuthorizationHandler = providerAuthorizationHandler;
            _employerAccountAuthorizationHandler = employerAccountAuthorizationHandler;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasProviderOrEmployerAccountRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext providerContext &&
                providerContext.RouteData.Values.ContainsKey(RouteValues.UkPrn))
            {
                if (!_providerAuthorizationHandler.IsProviderAuthorised(context))
                {
                    return Task.CompletedTask;
                }
            }

            if (context.Resource is AuthorizationFilterContext employerContext &&
                employerContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId))
            {
                if (!_employerAccountAuthorizationHandler.IsEmployerAuthorised(context))
                {
                    return Task.CompletedTask;
                }
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
