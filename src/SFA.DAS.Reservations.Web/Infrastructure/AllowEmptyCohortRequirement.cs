using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public class AllowEmptyCohortRequirement : IAuthorizationRequirement
{
}