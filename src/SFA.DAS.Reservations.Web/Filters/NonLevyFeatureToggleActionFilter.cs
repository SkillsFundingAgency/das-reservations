using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Web.AppStart;

namespace SFA.DAS.Reservations.Web.Filters
{
    public class NonLevyFeatureToggleActionFilter : ActionFilterAttribute
    {
        private readonly ServiceParameters _serviceParameters;
        private readonly IEncodingService _encodingService;
        private readonly IMediator _mediator;
        private readonly bool _featureToggleOn;

        public NonLevyFeatureToggleActionFilter(
            IConfiguration configuration,
            ServiceParameters serviceParameters,
            IEncodingService encodingService,
            IMediator mediator)
        {
            _serviceParameters = serviceParameters;
            _encodingService = encodingService;
            _mediator = mediator;
            var featureConfig = configuration["FeatureToggleOn"];

            if (!bool.TryParse(featureConfig, out var isFeatureEnabled))
            {
                isFeatureEnabled = true;
            }

            _featureToggleOn = isFeatureEnabled;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (_serviceParameters.AuthenticationType == AuthenticationType.Employer && !_featureToggleOn)
            {
                if (!context.RouteData.Values.TryGetValue("employerAccountId", out var employerAccountId))
                {
                    RedirectToTogglePage(context);
                    return;
                }

                if (!_encodingService.TryDecode(employerAccountId.ToString(), EncodingType.AccountId,
                    out var decodedAccountId))
                {
                    RedirectToTogglePage(context);
                    return;
                }

                var legalEntities = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = decodedAccountId });

                if (!legalEntities.AccountLegalEntities.Any(x => x.IsLevy))
                {
                    RedirectToTogglePage(context);
                    return;
                }
            }

            await next();
        }

        private static void RedirectToTogglePage(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"] as string ?? string.Empty;
            var actionName = context.RouteData.Values["action"] as string ?? string.Empty;

            if (!controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase) ||
                !actionName.Equals("FeatureNotAvailable", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("FeatureNotAvailable", "Home", new { });
            }
        }
    }
}
