using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization
{
    public interface IProviderAuthorisationHandler
    {
        bool IsProviderAuthorised(AuthorizationHandlerContext context);
    }
}
