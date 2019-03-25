using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Reservations.Web.Filters
{
    public class FeatureToggleActionFilter : IActionFilter
    {
        private readonly bool _featureToggleOn;

        public FeatureToggleActionFilter(IConfiguration configuration)
        {
            var featureConfig = configuration["FeatureToggleOn"];

            _featureToggleOn = bool.Parse(featureConfig);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_featureToggleOn) return;

            var controllerName = context.RouteData.Values["controller"] as string ?? string.Empty;
            var actionName = context.RouteData.Values["action"] as string ?? string.Empty;

            if (!controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase) ||
                !actionName.Equals("ServiceNotAvailable", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("ServiceNotAvailable", "Home", new { });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do something after the action executes
        }
    }
}
