using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Web.Infrastructure.Authorization
{
    public class EmployerAccountAuthorizationHandler : AuthorizationHandler<EmployerAccountRequirement>, IEmployerAccountAuthorisationHandler
    {
        private readonly IEmployerAccountService _accountsService;
        private readonly ILogger<EmployerAccountAuthorizationHandler> _logger;

        public EmployerAccountAuthorizationHandler(IEmployerAccountService accountsService, ILogger<EmployerAccountAuthorizationHandler> logger, IOptions<ReservationsWebConfiguration> configuration)
        {
            _accountsService = accountsService;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountRequirement requirement)
        {
            if (!IsEmployerAuthorised(context, false))
            {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }

        public bool IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles)
        {
            if (!(context.Resource is HttpContext mvcContext) || !mvcContext.Request.RouteValues.ContainsKey(RouteValues.EmployerAccountId))
                return false;

            var accountIdFromUrl = mvcContext.Request.RouteValues[RouteValues.EmployerAccountId].ToString().ToUpper();
            var employerAccountClaim = context.User.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

            if (employerAccountClaim?.Value == null)
                return false;

            Dictionary<string, EmployerIdentifier> employerAccounts;

            try
            {
                employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(employerAccountClaim.Value);
            }
            catch (JsonReaderException e)
            {
                _logger.LogError(e, "Could not deserialize employer account claim for user", employerAccountClaim.Value);
                return false;
            }

            EmployerIdentifier employerIdentifier = null;

            if (employerAccounts != null)
            {
                employerIdentifier = employerAccounts.ContainsKey(accountIdFromUrl)
                    ? employerAccounts[accountIdFromUrl] : null;
            }

            if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
            {
                if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
                    return false;

                var userClaim = context.User.Claims
                    .First(c => c.Type.Equals(ClaimTypes.NameIdentifier));
                var email = context.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;

                var userId = userClaim.Value;

                var updatedClaims = _accountsService.GetClaim(userId, EmployerClaims.AccountsClaimsTypeIdentifier, email).Result;
                var updatedAccountClaim = updatedClaims.First(c=>c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

                var updatedEmployerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(updatedAccountClaim.Value);

                userClaim.Subject.AddClaim(updatedAccountClaim);

                if (!updatedEmployerAccounts.ContainsKey(accountIdFromUrl))
                {
                    return false;
                }

                employerIdentifier = updatedEmployerAccounts[accountIdFromUrl];
            }

            if (!mvcContext.Items.ContainsKey(ContextItemKeys.EmployerIdentifier))
            {
                mvcContext.Items.Add(ContextItemKeys.EmployerIdentifier, employerAccounts.GetValueOrDefault(accountIdFromUrl));
            }

            if (!allowAllUserRoles && !CheckUserRoleForAccess(employerIdentifier))
            {
                return false;
            }

            return true;
        }

        private static bool CheckUserRoleForAccess(EmployerIdentifier employerIdentifier)
        {
            if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole))
            {
                return false;
            }

            return userRole == EmployerUserRole.Owner || userRole == EmployerUserRole.Transactor;
        }
    }
}