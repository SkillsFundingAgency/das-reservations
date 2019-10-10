using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Encoding;
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

        public LevyNotPermittedFilter(
            ServiceParameters serviceParameters,
            IEncodingService encodingService,
            IMediator mediator)
        {
            _serviceParameters = serviceParameters;
            _encodingService = encodingService;
            _mediator = mediator;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (_serviceParameters.AuthenticationType == AuthenticationType.Employer)
            {
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
