﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.ProviderAuthorizationHandlerTests
{
    public class WhenDeterminingIsAuthorised
    {
        [Test, MoqAutoData]
        public void ThenReturnsTrueIfProviderIsAuthorised(
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
            var result = handler.IsProviderAuthorised(context);

            //Assert
            Assert.IsTrue(result);
        }

        [Test, MoqAutoData]
        public void ThenReturnsFalseIfProviderUkprnNotInRoute(
            ProviderUkPrnRequirement requirement,
            [ArrangeDefaultHttpContextFilterContext] DefaultHttpContext contextFilter ,
            ProviderAuthorizationHandler handler)
        {
            //Assign
            var claim = new Claim(ProviderClaims.ProviderUkprn, "1234");
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});
            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            
            //Act
            var result = handler.IsProviderAuthorised(context);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenReturnsFalseIfUserDoesNotHaveClaim(
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
            var result = handler.IsProviderAuthorised(context);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenReturnsFalseIfUserDoesNotHaveMatchingUkprnInClaim(
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
            var result = handler.IsProviderAuthorised(context);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
