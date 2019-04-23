using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public interface IProviderAuthorisationHandler
    {
        bool IsProviderAuthorised(AuthorizationHandlerContext context);
    }
}
