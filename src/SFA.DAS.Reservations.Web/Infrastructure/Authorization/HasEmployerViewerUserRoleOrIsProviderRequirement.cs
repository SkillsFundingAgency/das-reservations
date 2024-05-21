using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization
{
    public class HasEmployerViewerUserRoleOrIsProviderRequirement : IAuthorizationRequirement
    {
    }
}
