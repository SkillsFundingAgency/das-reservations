using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Filters
{
    public class GoogleAnalyticsFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;

            if (context.HttpContext.User.HasClaim(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier)))
                controller.ViewBag.GaData = PopulateForEmployer(context);
            else
                controller.ViewBag.GaData = PopulateForProvider(context);

            base.OnActionExecuting(context);
        }

        private GaData PopulateForEmployer(ActionExecutingContext context)
        {
            string userId = null;
            string hashedAccountId = null;
            
            userId = context.HttpContext.User.Claims.First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier)).Value;

            if (!context.RouteData.Values.TryGetValue("employerAccountId", out var employerAccountId))
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
            string providerAccountId = null;

            ukPrn = context.HttpContext.User.Claims.First(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;

            if (!context.RouteData.Values.TryGetValue("providerAccountId", out var employerAccountId))
            {
                providerAccountId = employerAccountId.ToString();
            }

            return new GaData
            {
                UkPrn = ukPrn,
                Acc = providerAccountId
            };
        }

        public string DataLoaded { get; set; }

        public class GaData
        {
            public GaData()
            {

            }
            public string DataLoaded { get; set; } = "dataLoaded";
            public string UserId { get; set; }

            public string Vpv { get; set; }
            public string Acc { get; set; }
            public string UkPrn { get; set; }

            public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
        }
    }
}