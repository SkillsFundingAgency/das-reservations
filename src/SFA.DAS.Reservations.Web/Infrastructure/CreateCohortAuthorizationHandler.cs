using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Reservations.Web.Handlers;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public class CreateCohortAuthorizationHandler(IAccessCohortAuthorizationHelper helper) : AuthorizationHandler<CreateCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateCohortRequirement requirement)
    {
        if (!await helper.IsAuthorised())
        {
            return;
        }

        context.Succeed(requirement);
    }
}