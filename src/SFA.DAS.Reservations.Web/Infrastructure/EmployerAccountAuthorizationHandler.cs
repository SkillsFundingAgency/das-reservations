using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class EmployerAccountAuthorizationHandler : AuthorizationHandler<EmployerAccountRequirement>, IEmployerAccountAuthorisationHandler
    {
        private readonly IEmployerAccountService _accountsService;
        private readonly ILogger _logger;

        public EmployerAccountAuthorizationHandler(IEmployerAccountService accountsService, ILogger logger)
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
            if (!(context.Resource is AuthorizationFilterContext mvcContext) || !mvcContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId)) 
                return false;

            
            var accountIdFromUrl = mvcContext.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();
            var employerAccountClaim = context.User.FindFirst(c=>c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

            if(employerAccountClaim?.Value == null)
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
                if (!context.User.HasClaim(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier)))
                    return false;

                var userClaim = context.User.Claims
                    .First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier));

                var userId = userClaim.Value;

                var updatedAccountClaim = _accountsService.GetClaim(userId, EmployerClaims.AccountsClaimsTypeIdentifier).Result;

                var updatedEmployerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(updatedAccountClaim?.Value);

                userClaim.Subject.AddClaim(updatedAccountClaim);
                
                if (!updatedEmployerAccounts.ContainsKey(accountIdFromUrl))
                {
                    return false;
                }

                employerIdentifier = updatedEmployerAccounts[accountIdFromUrl];
            }

            if (!mvcContext.HttpContext.Items.ContainsKey(ContextItemKeys.EmployerIdentifier))
            {
                mvcContext.HttpContext.Items.Add(ContextItemKeys.EmployerIdentifier, employerAccounts.GetValueOrDefault(accountIdFromUrl));
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