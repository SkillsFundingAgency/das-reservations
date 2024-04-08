using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Reservations.Web.Handlers;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization;

public class CreateCohortAuthorizationHandler(ICreateCohortAuthorizationHelper helper) : AuthorizationHandler<CreateCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateCohortRequirement requirement)
    {
        if (!await helper.CanCreateCohort())
        {
            return;
        }

        context.Succeed(requirement);
    }
}