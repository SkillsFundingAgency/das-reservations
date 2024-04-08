using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class HasEmployerViewerUserRoleOrIsProvider : AuthorizationHandler<HasEmployerViewerUserRoleOrIsProviderRequirement>
    {
       
        private readonly IEmployerAccountAuthorisationHandler _employerAccountAuthorizationHandler;
        private readonly IProviderAuthorisationHandler _providerAuthorizationHandler;

        public HasEmployerViewerUserRoleOrIsProvider(IEmployerAccountAuthorisationHandler employerAccountAuthorizationHandler,
            IProviderAuthorisationHandler providerAuthorizationHandler)
        {
            _employerAccountAuthorizationHandler = employerAccountAuthorizationHandler;
            _providerAuthorizationHandler = providerAuthorizationHandler;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasEmployerViewerUserRoleOrIsProviderRequirement orIsProviderRequirement)
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
                if (!_employerAccountAuthorizationHandler.IsEmployerAuthorised(context, true))
                {
                    return Task.CompletedTask;
                }
            }

            context.Succeed(orIsProviderRequirement);

            return Task.CompletedTask;
        }
    }
}
