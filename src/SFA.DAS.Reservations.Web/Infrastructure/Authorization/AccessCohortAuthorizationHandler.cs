using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization.Helpers;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization;

public class AccessCohortAuthorizationHandler(IAccessCohortAuthorizationHelper accessCohortAuthorizationHelper,
    ICreateCohortAuthorizationHelper createCohortAuthorizationHelper,
    IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<AccessCohortRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessCohortRequirement requirement)
    {
        if (UrlHasCohortReference())
        {
            if (!await accessCohortAuthorizationHelper.CanAccessCohort())
            {
                return;
            }
        }
        else
        {
            if (!await createCohortAuthorizationHelper.CanCreateCohort())
            {
                return;
            }
        }

        context.Succeed(requirement);
    }

    private bool UrlHasCohortReference()
    {
        return httpContextAccessor.HttpContext.TryGetValueFromHttpContext(RouteValueKeys.CohortReference, out var _);
    }
}