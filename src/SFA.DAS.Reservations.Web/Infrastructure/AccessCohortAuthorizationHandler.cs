using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public class AccessCohortAuthorizationHandler(ICommitmentsAuthorisationHandler handler) : AuthorizationHandler<AccessCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessCohortRequirement requirement)
    {
        if (!await handler.CanAccessCohort())
        {
            return;
        }

        context.Succeed(requirement);
    }
}

public class AllowEmptyCohortAuthorizationHandler(ICommitmentsAuthorisationHandler handler) : AuthorizationHandler<AllowEmptyCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowEmptyCohortRequirement requirement)
    {
        if (!await handler.AllowEmptyCohort())
        {
            return;
        }

        context.Succeed(requirement);
    }
}