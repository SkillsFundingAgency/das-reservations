﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class ProviderAuthorizationHandler : AuthorizationHandler<ProviderUkPrnRequirement>, IProviderAuthorisationHandler
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProviderUkPrnRequirement requirement)
        {
            if (!IsProviderAuthorised(context))
            {
                return Task.CompletedTask;
            }
                
            context.Succeed(requirement);

            return Task.CompletedTask;
        }

        public bool IsProviderAuthorised(AuthorizationHandlerContext context)
        {
            if (!(context.Resource is AuthorizationFilterContext mvcContext) || !mvcContext.RouteData.Values.ContainsKey(RouteValues.UkPrn))
                return false;

            if (!context.User.HasClaim(c => c.Type.Equals(ProviderClaims.ProviderUkprn)))
                return false;

            var ukPrnFromUrl = mvcContext.RouteData.Values[RouteValues.UkPrn].ToString().ToUpper();
            var ukPrn = context.User.FindFirst(c => c.Type.Equals(ProviderClaims.ProviderUkprn)).Value;

            if (ukPrn == ukPrnFromUrl)
            {
                return true;
            }
            
            context.Fail();
                
            return false;
        }
    }
}