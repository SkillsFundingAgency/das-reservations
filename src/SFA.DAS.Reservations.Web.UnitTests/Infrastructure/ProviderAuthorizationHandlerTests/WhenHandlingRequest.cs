using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.ProviderAuthorizationHandlerTests
{
    class WhenHandlingRequest
    {
        [Test, MoqAutoData]
        public async Task ThenSucceedsIfProviderIsAuthorised(
            ProviderUkPrnRequirement requirement,
            AuthorizationFilterContext contextFilter,
            ProviderAuthorizationHandler handler)
        {
            //Assign
            var claim = new Claim(ProviderClaims.ProviderUkprn, "1234");
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.UkPrn, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsTrue(context.HasSucceeded);
        }

        [Test, MoqAutoData]
        public async Task ThenFailsIfProviderUkprnNotInRoute(
            ProviderUkPrnRequirement requirement,
            AuthorizationFilterContext contextFilter,
            ProviderAuthorizationHandler handler)
        {
            //Assign
            var claim = new Claim(ProviderClaims.ProviderUkprn, "1234");
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, MoqAutoData]
        public async Task ThenFailsIfUserDoesNotHaveClaim(
            ProviderUkPrnRequirement requirement,
            AuthorizationFilterContext contextFilter,
            ProviderAuthorizationHandler handler)
        {
            //Assign
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new Claim[0])});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.UkPrn, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, MoqAutoData]
        public async Task ThenFailsIfUserDoesNotHaveMatchingUkprnInClaim(
            ProviderUkPrnRequirement requirement,
            AuthorizationFilterContext contextFilter,
            ProviderAuthorizationHandler handler)
        {
            //Assign
            var claim = new Claim(ProviderClaims.ProviderUkprn, "5555");
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.UkPrn, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }
    }
}
