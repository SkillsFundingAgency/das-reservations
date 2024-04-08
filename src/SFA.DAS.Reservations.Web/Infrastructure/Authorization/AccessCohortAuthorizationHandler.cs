using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization.Helpers;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization;

public class AccessCohortAuthorizationHandler(IAccessCohortAuthorizationHelper accessCohortAuthorizationHelper) : AuthorizationHandler<AccessCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessCohortRequirement requirement)
    {
        if (!await accessCohortAuthorizationHelper.CanAccessCohort())
        {
            return;
        }

        context.Succeed(requirement);
    }
}