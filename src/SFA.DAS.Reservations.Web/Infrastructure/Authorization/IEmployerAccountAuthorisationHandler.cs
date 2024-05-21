using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization
{
    public interface IEmployerAccountAuthorisationHandler
    {
        bool IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles);
    }
}
