using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Extensions;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Filters
{
    public class LevyNotPermittedFilter : ActionFilterAttribute
    {
        private readonly ServiceParameters _serviceParameters;
        private readonly IEncodingService _encodingService;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public LevyNotPermittedFilter(
            ServiceParameters serviceParameters,
            IEncodingService encodingService,
            IMediator mediator,
            IConfiguration configuration)
        {
            _serviceParameters = serviceParameters;
            _encodingService = encodingService;
            _mediator = mediator;
            _configuration = configuration;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (_serviceParameters.AuthenticationType == AuthenticationType.Employer)
            {
                var isAccountSuspended = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value;
                if (isAccountSuspended != null && isAccountSuspended.Equals("Suspended", StringComparison.CurrentCultureIgnoreCase))
                {
                    context.HttpContext.Response.Redirect(RedirectExtension.GetAccountSuspendedRedirectUrl(_configuration["ResourceEnvironmentName"]));
                    return;
                }
                
                
                if (!context.RouteData.Values.TryGetValue("employerAccountId", out var employerAccountId))
                {
                    context.Result = new RedirectToRouteResult(RouteNames.Error500, null);
                    return;
                }

                if (!_encodingService.TryDecode(employerAccountId.ToString(), EncodingType.AccountId,
                    out var decodedAccountId))
                {
                    context.Result = new RedirectToRouteResult(RouteNames.Error403, null);
                    return;
                }

                var legalEntities = await _mediator.Send(new GetLegalEntitiesQuery { AccountId = decodedAccountId });

                if (legalEntities.AccountLegalEntities.Any(x => x.IsLevy))
                {
                    context.Result = new RedirectToRouteResult(RouteNames.Error403, null);
                    return;
                }
            }

            await next();
        }
    }
}
