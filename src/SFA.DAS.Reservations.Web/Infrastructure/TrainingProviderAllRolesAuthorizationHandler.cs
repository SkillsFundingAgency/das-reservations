using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Web.AppStart;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class TrainingProviderAllRolesAuthorizationHandler : AuthorizationHandler<TrainingProviderAllRolesRequirement>
    {
        private readonly ServiceParameters _serviceParameters;
        private readonly ITrainingProviderAuthorizationHandler _handler;
        private readonly IConfiguration _configuration;
        private readonly ReservationsWebConfiguration _reservationsWebConfiguration;

        public TrainingProviderAllRolesAuthorizationHandler(
            ServiceParameters serviceParameters,
            ITrainingProviderAuthorizationHandler handler,
            IConfiguration configuration,
            IOptions<ReservationsWebConfiguration> reservationsWebConfiguration)
        {
            _serviceParameters = serviceParameters;
            _handler = handler;
            _reservationsWebConfiguration = reservationsWebConfiguration.Value;
            _configuration = configuration;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TrainingProviderAllRolesRequirement requirement)
        {
            if (_serviceParameters.AuthenticationType == AuthenticationType.Employer)
            {
                context.Succeed(requirement);
                return;
            }
            
            HttpContext currentContext;
            switch (context.Resource)
            {
                case HttpContext resource:
                    currentContext = resource;
                    break;
                case AuthorizationFilterContext authorizationFilterContext:
                    currentContext = authorizationFilterContext.HttpContext;
                    break;
                default:
                    currentContext = null;
                    break;
            }

            if (!context.User.HasClaim(c => c.Type.Equals(ProviderClaims.ProviderUkprn)))
            {
                context.Fail();
                return;
            }

            var claimValue = context.User.FindFirst(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;

            if (!int.TryParse(claimValue, out var ukprn))
            {
                context.Fail();
                return;
            }

            var isStubProviderValidationEnabled = GetUseStubProviderValidationSetting();

            // check if the stub is activated to by-pass the validation. Mostly used for local development purpose.
            // logic to check if the provider is authorized if not redirect the user to PAS 401 un-authorized page.
            if (!isStubProviderValidationEnabled && !(await _handler.IsProviderAuthorized(context)))
            {
                currentContext?.Response.Redirect($"{_reservationsWebConfiguration.DashboardUrl}/error/403/invalid-status");
            }

            context.Succeed(requirement);
        }

        private bool GetUseStubProviderValidationSetting()
        {
            var value = _configuration.GetSection("UseStubProviderValidation").Value;

            return value != null && bool.TryParse(value, out var result) && result;
        }
    }
}
