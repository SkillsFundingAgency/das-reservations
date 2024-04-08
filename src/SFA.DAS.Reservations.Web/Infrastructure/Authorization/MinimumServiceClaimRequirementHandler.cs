using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization
{
    public class MinimumServiceClaimRequirementHandler : AuthorizationHandler<MinimumServiceClaimRequirement>
    {
        private readonly ILogger<MinimumServiceClaimRequirementHandler> _logger;

        public MinimumServiceClaimRequirementHandler(ILogger<MinimumServiceClaimRequirementHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumServiceClaimRequirement requirement)
        {
            _logger.LogDebug("In Handler requirement for minimum service claim " + requirement.MinimumServieClaim);
            if (context.Resource is HttpContext  employerContext &&
            employerContext.Request.RouteValues.ContainsKey(RouteValues.EmployerAccountId))
            {
                _logger.LogDebug("It is for employer, bypassing the rules for provider service claim");
                // Marking this as succeeded as there are other handler which checks for whether employer is authorized.
                context.Succeed(requirement);
            }
            else
            {
                var highestServiceClaim = GetHighetServiceClaim(context);

                if (highestServiceClaim.HasValue)
                {
                    _logger.LogDebug("claim that user has:" + highestServiceClaim);
                    _logger.LogDebug("minimum claim required: " + requirement.MinimumServieClaim);

                    if (HasValidServiceClaim(highestServiceClaim.Value, requirement))
                    {
                        _logger.LogDebug("verified minimum claim requirement");
                        context.Succeed(requirement);
                    }
                    else
                    {
                        _logger.LogDebug("failed minimum claim requirement");
                    }
                }
                else
                {
                    _logger.LogDebug("Highetst service claim is null");
                }
            }

            return Task.CompletedTask;
        }

        private bool HasValidServiceClaim(ServiceClaim highestServiceClaim, MinimumServiceClaimRequirement requirement)
        {
            return highestServiceClaim >= requirement.MinimumServieClaim;
        }

        private ServiceClaim? GetHighetServiceClaim(AuthorizationHandlerContext context)
        {
            if (context.User.HasClaim(c => c.Type.Equals(ProviderClaims.Service) && c.Value == "DAA"))
            {
                return ServiceClaim.DAA;
            }
            else if (context.User.HasClaim(c => c.Type.Equals(ProviderClaims.Service) && c.Value == "DAB"))
            {
                return ServiceClaim.DAB;
            }
            else if (context.User.HasClaim(c => c.Type.Equals(ProviderClaims.Service) && c.Value == "DAC"))
            {
                return ServiceClaim.DAC;
            }
            else if (context.User.HasClaim(c => c.Type.Equals(ProviderClaims.Service) && c.Value == "DAV"))
            {
                return ServiceClaim.DAV;
            }

            return null;
        }
    }
}