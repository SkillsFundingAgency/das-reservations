using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntity;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Infrastructure;

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
            if (!_featureToggleOn)
            {
                if (!context.RouteData.Values.TryGetValue("accountLegalEntityPublicHashedId", out var legalEntityHashedId))
                {
                    RedirectToTogglePage(context);
                    return;
                }

                if (!_encodingService.TryDecode(legalEntityHashedId.ToString(), EncodingType.PublicAccountLegalEntityId,
                    out var legalEntityId))
                {
                    RedirectToTogglePage(context);
                    return;
                }

                var response = await _mediator.Send(new GetLegalEntityQuery { Id = legalEntityId });

                if (response?.AccountLegalEntity == null || !response.AccountLegalEntity.IsLevy)
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
