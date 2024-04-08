using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization.Helpers;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization;

public class CreateCohortAuthorizationHandler(ICreateCohortAuthorizationHelper createCohortAuthorizationHelper) : AuthorizationHandler<CreateCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateCohortRequirement requirement)
    {
        if (!await createCohortAuthorizationHelper.CanCreateCohort())
        {
            return;
        }

        context.Succeed(requirement);
    }
}