using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Models;
using System.Linq;

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
            string levyFlag = null;

            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))?.Value;

            if (context.RouteData.Values.TryGetValue("employerAccountId", out var employerAccountId))
            {
                hashedAccountId = employerAccountId.ToString();

                var accountsJson = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.AssociatedAccounts))?.Value;
                if ( accountsJson is not null)
                {
                    var accounts = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, EmployerIdentifier>>(accountsJson);
                    levyFlag = accounts.TryGetValue(hashedAccountId, out var employer) ? employer.ApprenticeshipEmployerType.ToString() : null;
                }                
            }

            return new GaData
            {
                UserId = userId,
                Acc = hashedAccountId,
                LevyFlag = levyFlag
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