using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Reservations.Web.Extensions;

namespace SFA.DAS.Reservations.Web.AppStart;

public class KeepAliveControllerConvention(IConfiguration configuration) : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        var isEmployerAuth = configuration.IsEmployerAuth();
        var isProviderAuth = configuration.IsProviderAuth();

        var controllersToRemove = application.Controllers
            .Where(c => c.ControllerName == "SessionKeepAlive" && 
                       ((isEmployerAuth && c.ControllerType.Namespace?.Contains("SFA.DAS.DfESignIn.Auth") == true) ||
                        (isProviderAuth && c.ControllerType.Namespace?.Contains("SFA.DAS.GovUK.Auth") == true)))
            .ToList();

        foreach (var controller in controllersToRemove)
        {
            application.Controllers.Remove(controller);
        }
    }
} 