using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Filters
{
    public class NonEoiNotPermittedFilterAttribute : ActionFilterAttribute
    {
        private readonly ServiceParameters _serviceParameters;
        private readonly IEncodingService _encodingService;
        private readonly IMediator _mediator;
        private readonly IExternalUrlHelper _urlHelper;
        private readonly ILogger<NonEoiNotPermittedFilterAttribute> _logger;

        public NonEoiNotPermittedFilterAttribute(
            ServiceParameters serviceParameters,
            IEncodingService encodingService,
            IMediator mediator,
            IExternalUrlHelper urlHelper,
            ILogger<NonEoiNotPermittedFilterAttribute> logger)
        {
            _serviceParameters = serviceParameters;
            _encodingService = encodingService;
            _mediator = mediator;
            _urlHelper = urlHelper;
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (_serviceParameters.AuthenticationType != AuthenticationType.Employer)
            {
                await next();
                return;
            }

            if (!context.RouteData.Values.TryGetValue("employerAccountId", out var employerAccountId))
            {
                //context.res
            }

            var decodedAccountId = _encodingService.Decode(employerAccountId?.ToString(), EncodingType.AccountId);
            var result = await _mediator.Send(new GetLegalEntitiesQuery
            {
                AccountId = decodedAccountId
            });

            if (result.AccountLegalEntities.Any(entity =>
                    !entity.IsLevy && 
                    entity.AgreementType != AgreementType.NonLevyExpressionOfInterest) || 
                !result.AccountLegalEntities.Any())
            {
                var homeLink = _urlHelper.GenerateUrl(new UrlParameters
                {
                    Controller = "teams",
                    SubDomain = "accounts",
                    Folder = "accounts",
                    Id = employerAccountId.ToString()
                });

                var model = new NonEoiHoldingViewModel
                {
                    HomeLink = homeLink
                };

                var viewResult = new ViewResult
                {
                    ViewName = "NonEoiHolding", 
                    ViewData = new ViewDataDictionary(((Controller)context.Controller).ViewData)
                    {
                        Model = model
                    }
                };
                context.Result = viewResult;
                return;
            }

            await next();
        }
    }
}