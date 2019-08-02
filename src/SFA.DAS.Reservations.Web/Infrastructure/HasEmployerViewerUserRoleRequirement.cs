using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class HasEmployerViewerUserRoleRequirement : IAuthorizationRequirement
    {
    }
}
