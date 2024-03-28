using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Reservations.Web.Handlers;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public class AccessCohortAuthorizationHandler(IAccessCohortAuthorizationHelper helper) : AuthorizationHandler<AccessCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessCohortRequirement requirement)
    {
        if (!await helper.IsAuthorised(context))
        {
            return;
        }

        context.Succeed(requirement);
    }
}