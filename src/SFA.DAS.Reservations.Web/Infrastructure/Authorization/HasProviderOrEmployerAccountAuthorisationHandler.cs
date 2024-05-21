using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization
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
            if (context.Resource is HttpContext providerContext &&
                providerContext.Request.RouteValues.ContainsKey(RouteValues.UkPrn))
            {
                if (!_providerAuthorizationHandler.IsProviderAuthorised(context))
                {
                    return Task.CompletedTask;
                }
            }

            if (context.Resource is HttpContext employerContext &&
                employerContext.Request.RouteValues.ContainsKey(RouteValues.EmployerAccountId))
            {
                if (!_employerAccountAuthorizationHandler.IsEmployerAuthorised(context, false))
                {
                    return Task.CompletedTask;
                }
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
