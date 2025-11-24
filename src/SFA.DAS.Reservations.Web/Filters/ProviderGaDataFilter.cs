using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Provider.Shared.UI.Models;
using SFA.DAS.Reservations.Web.AppStart;

namespace SFA.DAS.Reservations.Web.Filters;

public class ProviderGaDataFilter : IAsyncActionFilter
{
    private readonly ServiceParameters _serviceParameters;

    public ProviderGaDataFilter(ServiceParameters serviceParameters)
    {
        _serviceParameters = serviceParameters;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is Controller controller && 
            _serviceParameters.AuthenticationType == AuthenticationType.Provider)
        {
            // Populate ViewBag.GaData with provider shared UI's GaData model
            var gaData = new GaData
            {
                DataLoaded = "dataLoaded",
                UkPrn = context.RouteData.Values.TryGetValue("ukPrn", out var ukPrn) 
                    ? ukPrn.ToString() 
                    : null,
                Vpv = controller.ViewBag.GaData?.Vpv, // Preserve Vpv if already set
                UserId = context.HttpContext.User?.Identity?.Name,
                Org = null, // Can be set if needed
                Extras = new Dictionary<string, string>()
            };

            // If there's already a GaData in ViewBag (from GoogleAnalyticsFilter), preserve Vpv
            if (controller.ViewBag.GaData != null && controller.ViewBag.GaData.GetType().GetProperty("Vpv") != null)
            {
                var existingVpv = controller.ViewBag.GaData.GetType().GetProperty("Vpv")?.GetValue(controller.ViewBag.GaData)?.ToString();
                if (!string.IsNullOrEmpty(existingVpv))
                {
                    gaData.Vpv = existingVpv;
                }
            }

            controller.ViewBag.GaData = gaData;
        }

        await next();
    }
}

