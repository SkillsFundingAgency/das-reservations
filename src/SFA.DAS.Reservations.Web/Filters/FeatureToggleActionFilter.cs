using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Filters
{
    public class FeatureToggleActionFilter : ActionFilterAttribute
    {
        private readonly bool _featureToggleOn;

        public FeatureToggleActionFilter(IConfiguration configuration)
        {
            var featureConfig = configuration["FeatureToggleOn"];

            if (!bool.TryParse(featureConfig, out var isFeatureEnabled))
            {
                isFeatureEnabled = true;
            }

            _featureToggleOn = isFeatureEnabled;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_featureToggleOn)
            {
                return;
            }

            var controllerName = context.RouteData.Values["controller"] as string ?? string.Empty;
            var actionName = context.RouteData.Values["action"] as string ?? string.Empty;

            if (!controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase) ||
                !actionName.Equals("FeatureNotAvailable", StringComparison.OrdinalIgnoreCase))
            {
                if(context.RouteData.Values.TryGetValue("employerAccountId", out var employerAccountId))
                {
                    context.Result = new RedirectToRouteResult(RouteNames.EmployerFeatureNotAvailable, new {employerAccountId});
                }
                else  if(context.RouteData.Values.TryGetValue("ukprn", out var ukprn))
                {
                    context.Result = new RedirectToRouteResult(RouteNames.ProviderFeatureNotAvailable,new {ukprn});
                }
                else
                {
                    context.Result = new RedirectToRouteResult(RouteNames.Error403, null);
                }
            }
        }

      
    }
}
