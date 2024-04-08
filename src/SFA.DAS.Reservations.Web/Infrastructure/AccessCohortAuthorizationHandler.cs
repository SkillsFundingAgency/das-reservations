using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Handlers;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public class AccessCohortAuthorizationHandler(ICommitmentsAuthorisationHandler commitmentsAuthorisationHandler, IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<AccessCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessCohortRequirement requirement)
    {
        if (httpContextAccessor.HttpContext.User.IsEmployer())
        {
            context.Succeed(requirement);
            return;
        }

        if (!await commitmentsAuthorisationHandler.CanAccessCohort())
        {
            return;
        }

        context.Succeed(requirement);
    }
}