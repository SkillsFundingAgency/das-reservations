using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Filters
{
    public class GoogleAnalyticsFilter : ActionFilterAttribute
    {
        private readonly ServiceParameters _serviceParameters;

        public GoogleAnalyticsFilter (ServiceParameters serviceParameters)
        {
            _serviceParameters = serviceParameters;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.Controller is Controller controller))
            {
                return;
            }
            
            controller.ViewBag.GaData = _serviceParameters.AuthenticationType == AuthenticationType.Employer 
                ? PopulateForEmployer(context) : PopulateForProvider(context);

            base.OnActionExecuting(context);
        }

        private GaData PopulateForEmployer(ActionExecutingContext context)
        {
            string hashedAccountId = null;
            
            var userId = context.HttpContext.User.Claims.First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier)).Value;

            if (context.RouteData.Values.TryGetValue("employerAccountId", out var employerAccountId))
            {
                hashedAccountId = employerAccountId.ToString();
            }

            return new GaData
            {
                UserId = userId,
                Acc = hashedAccountId
            };
        }

        private GaData PopulateForProvider(ActionExecutingContext context)
        {
            string ukPrn = null;

            if (context.RouteData.Values.TryGetValue("ukPrn", out var providerId))
            {
                ukPrn = providerId.ToString();
            }

            return new GaData
            {
                UkPrn = ukPrn
            };
        }

        public string DataLoaded { get; set; }

        
    }
}