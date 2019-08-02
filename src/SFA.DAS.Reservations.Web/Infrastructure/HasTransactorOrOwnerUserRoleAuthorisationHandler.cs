using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class HasTransactorOrOwnerUserRoleAuthorisationHandler : AuthorizationHandler<HasTransactorOrOwnerUserRoleRequirement>
    {
        private readonly IEmployerAccountService _accountsService;
        private readonly IProviderAuthorisationHandler _providerAuthorizationHandler;
        private readonly IEmployerAccountAuthorisationHandler _employerAccountAuthorizationHandler;

        public HasTransactorOrOwnerUserRoleAuthorisationHandler(IEmployerAccountService accountsService,IProviderAuthorisationHandler providerAuthorizationHandler,
            IEmployerAccountAuthorisationHandler employerAccountAuthorizationHandler)
        {
            _accountsService = accountsService;
            _providerAuthorizationHandler = providerAuthorizationHandler;
            _employerAccountAuthorizationHandler = employerAccountAuthorizationHandler;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasTransactorOrOwnerUserRoleRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext providerContext &&
                providerContext.RouteData.Values.ContainsKey(RouteValues.UkPrn))
            {
                if (_providerAuthorizationHandler.IsProviderAuthorised(context))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            if (context.Resource is AuthorizationFilterContext employerContext &&
                employerContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId))
            {
                if (!_employerAccountAuthorizationHandler.IsEmployerAuthorised(context))
                {
                    return Task.CompletedTask;
                }
            }
            
            //--------

            if (!(context.Resource is AuthorizationFilterContext mvcContext) || !mvcContext.RouteData.Values.ContainsKey(RouteValues.EmployerAccountId)) 
                return Task.CompletedTask;

            if (!context.User.HasClaim(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)))
                return Task.CompletedTask;
            
            var accountIdFromUrl = mvcContext.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();
            var employerAccountClaim = context.User.FindFirst(c=>c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
            var employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(employerAccountClaim?.Value);
            
            if (employerAccountClaim == null)
                return Task.CompletedTask;

            EmployerIdentifier employerIdentifier = null;

            if (employerAccounts != null)
            {
                employerIdentifier = employerAccounts[accountIdFromUrl];
            }

            if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
            {
                if (!context.User.HasClaim(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier)))
                    return Task.CompletedTask;

                var userClaim = context.User.Claims
                    .First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier));

                var userId = userClaim.Value;

                var updatedAccountClaim = _accountsService.GetClaim(userId, EmployerClaims.AccountsClaimsTypeIdentifier)
                    .Result;

                var updatedEmployerAccounts =
                    JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(updatedAccountClaim?.Value);

                userClaim.Subject.AddClaim(updatedAccountClaim);

                if (!updatedEmployerAccounts.ContainsKey(accountIdFromUrl))
                {
                    return Task.CompletedTask;
                }

                employerIdentifier = updatedEmployerAccounts[accountIdFromUrl];
            }

            if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole))
            {
                return Task.CompletedTask;
            }

            if (userRole != EmployerUserRole.Owner && userRole != EmployerUserRole.Transactor)
            {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
