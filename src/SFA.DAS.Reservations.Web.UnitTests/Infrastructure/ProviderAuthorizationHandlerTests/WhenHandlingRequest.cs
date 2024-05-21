using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.ProviderAuthorizationHandlerTests
{
    class WhenHandlingRequest
    {
        [Test, MoqAutoData]
        public async Task ThenSucceedsIfProviderIsAuthorised(
            ProviderUkPrnRequirement requirement,
            [ArrangeDefaultHttpContextFilterContext] DefaultHttpContext contextFilter ,
            ProviderAuthorizationHandler handler)
        {
            //Assign
            var claim = new Claim(ProviderClaims.ProviderUkprn, "1234");
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as DefaultHttpContext;
            filter.HttpContext.Request.RouteValues.Add(RouteValues.UkPrn, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsTrue(context.HasSucceeded);
        }

        [Test, MoqAutoData]
        public async Task ThenFailsIfProviderUkprnNotInRoute(
            ProviderUkPrnRequirement requirement,
            [ArrangeDefaultHttpContextFilterContext] DefaultHttpContext contextFilter ,
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
            [ArrangeDefaultHttpContextFilterContext] DefaultHttpContext contextFilter ,
            ProviderAuthorizationHandler handler)
        {
            //Assign
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new Claim[0])});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as DefaultHttpContext;
            filter.HttpContext.Request.RouteValues.Add(RouteValues.UkPrn, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, MoqAutoData]
        public async Task ThenFailsIfUserDoesNotHaveMatchingUkprnInClaim(
            ProviderUkPrnRequirement requirement,
            [ArrangeDefaultHttpContextFilterContext] DefaultHttpContext contextFilter ,
            ProviderAuthorizationHandler handler)
        {
            //Assign
            var claim = new Claim(ProviderClaims.ProviderUkprn, "5555");
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as DefaultHttpContext;
            filter.HttpContext.Request.RouteValues.Add(RouteValues.UkPrn, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }
    }
}
