using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.HasProviderOrEmployerAccountAuthorisationHandlerTests
{
    public class WhenHandlingRequest
    {
        [Test, MoqAutoData]
        public async Task ThenChecksIfProviderIsAuthorised(
            [Frozen] Mock<IProviderAuthorisationHandler> providerAuthorizationHandler,
            HasProviderOrEmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
            HasProviderOrEmployerAccountAuthorisationHandler handler)
        {
            //Assign
            var context = new AuthorizationHandlerContext(new []{requirement},ClaimsPrincipal.Current, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add("ukprn", 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            providerAuthorizationHandler.Verify(h => h.IsProviderAuthorised(context), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ThenChecksIfEmployerIsAuthorised(
            [Frozen] Mock<IEmployerAccountAuthorisationHandler> employerAccountAuthorizationHandler,
            HasProviderOrEmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
            HasProviderOrEmployerAccountAuthorisationHandler handler)
        {
            //Assign
            var context = new AuthorizationHandlerContext(new []{requirement},ClaimsPrincipal.Current, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            employerAccountAuthorizationHandler.Verify(h => h.IsEmployerAuthorised(context, false), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ThenFailsCheckIfProviderIsNotAuthorised(
            [Frozen] Mock<IProviderAuthorisationHandler> providerAuthorizationHandler,
            HasProviderOrEmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
            HasProviderOrEmployerAccountAuthorisationHandler handler)
        {
            //Assign
            var context = new AuthorizationHandlerContext(new []{requirement},ClaimsPrincipal.Current, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add("ukprn", 1234);

            providerAuthorizationHandler
                .Setup(h => h.IsProviderAuthorised(It.IsAny<AuthorizationHandlerContext>()))
                .Returns(false);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, MoqAutoData]
        public async Task ThenFailsCheckIfEmployerIsNotAuthorised(
            [Frozen] Mock<IEmployerAccountAuthorisationHandler> employerAccountAuthorizationHandler,
            HasProviderOrEmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
            HasProviderOrEmployerAccountAuthorisationHandler handler)
        {
            //Assign
            var context = new AuthorizationHandlerContext(new []{requirement},ClaimsPrincipal.Current, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            employerAccountAuthorizationHandler
                .Setup(h => h.IsEmployerAuthorised(It.IsAny<AuthorizationHandlerContext>(), It.IsAny<bool>()))
                .Returns(false);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

          [Test, MoqAutoData]
        public async Task ThenPassesCheckIfProviderIsNotAuthorised(
            [Frozen] Mock<IProviderAuthorisationHandler> providerAuthorizationHandler,
            HasProviderOrEmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
            HasProviderOrEmployerAccountAuthorisationHandler handler)
        {
            //Assign
            var context = new AuthorizationHandlerContext(new []{requirement},ClaimsPrincipal.Current, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add("ukprn", 1234);

            providerAuthorizationHandler
                .Setup(h => h.IsProviderAuthorised(It.IsAny<AuthorizationHandlerContext>()))
                .Returns(true);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsTrue(context.HasSucceeded);
        }

        [Test, MoqAutoData]
        public async Task ThenPassesCheckIfEmployerIsNotAuthorised(
            [Frozen] Mock<IEmployerAccountAuthorisationHandler> employerAccountAuthorizationHandler,
            HasProviderOrEmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
            HasProviderOrEmployerAccountAuthorisationHandler handler)
        {
            //Assign
            var context = new AuthorizationHandlerContext(new []{requirement},ClaimsPrincipal.Current, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            employerAccountAuthorizationHandler
                .Setup(h => h.IsEmployerAuthorised(It.IsAny<AuthorizationHandlerContext>(), It.IsAny<bool>()))
                .Returns(true);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsTrue(context.HasSucceeded);
        }
    }
}
