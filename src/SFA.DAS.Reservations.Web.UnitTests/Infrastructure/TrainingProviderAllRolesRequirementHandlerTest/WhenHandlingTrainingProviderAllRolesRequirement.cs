using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.TrainingProviderAllRolesRequirementHandlerTest
{
    public class WhenHandlingTrainingProviderAllRolesRequirement
    {
        [Test, MoqAutoData]
        public async Task Then_Succeeds_For_Employer_Auth(
            int ukprn,
            [Frozen] ServiceParameters serviceParameters,
            TrainingProviderAllRolesRequirement providerRequirement,
            TrainingProviderAllRolesAuthorizationHandler authorizationHandler)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            var claim = new Claim("NotProviderClaim", ukprn.ToString());
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
            var context = new AuthorizationHandlerContext(new[] { providerRequirement }, claimsPrinciple, null);

            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            Assert.IsTrue(context.HasSucceeded);
            Assert.IsFalse(context.HasFailed);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Fails_If_No_Provider_Ukprn_Claim(
        int ukprn,
        [Frozen] ServiceParameters serviceParameters,
        TrainingProviderAllRolesRequirement providerRequirement,
        TrainingProviderAllRolesAuthorizationHandler authorizationHandler)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Provider;
            var claim = new Claim("NotProviderClaim", ukprn.ToString());
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
            var context = new AuthorizationHandlerContext(new[] { providerRequirement }, claimsPrinciple, null);

            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
            Assert.IsTrue(context.HasFailed);
        }

        [Test, MoqAutoData]
        public async Task Then_Fails_If_Non_Numeric_Provider_Ukprn_Claim(
            string ukprn,
            [Frozen] ServiceParameters serviceParameters,
            TrainingProviderAllRolesRequirement providerRequirement,
            TrainingProviderAllRolesAuthorizationHandler authorizationHandler)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Provider;
            var claim = new Claim(ProviderClaims.ProviderUkprn, ukprn);
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
            var context = new AuthorizationHandlerContext(new[] { providerRequirement }, claimsPrinciple, null);

            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
            Assert.IsTrue(context.HasFailed);
        }

        [Test, MoqAutoData]
        public async Task Then_Fails_If_Provider_Ukprn_Claim_Response_Is_False(
            int ukprn,
            [Frozen] ServiceParameters serviceParameters,
            TrainingProviderAllRolesRequirement providerRequirement,
            [Frozen] Mock<ITrainingProviderAuthorizationHandler> trainingProviderAuthorizationHandler,
            TrainingProviderAllRolesAuthorizationHandler authorizationHandler)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Provider;
            var httpContextBase = new Mock<HttpContext>();
            var httpResponse = new Mock<HttpResponse>();
            httpContextBase.Setup(c => c.Response).Returns(httpResponse.Object);
            var filterContext = new AuthorizationFilterContext(new ActionContext(httpContextBase.Object, new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>());
            var claim = new Claim(ProviderClaims.ProviderUkprn, ukprn.ToString());
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
            var context = new AuthorizationHandlerContext(new[] { providerRequirement }, claimsPrinciple, filterContext);
            var response = new ProviderAccountResponse { CanAccessService = false };
            trainingProviderAuthorizationHandler.Setup(x => x.IsProviderAuthorized(context)).ReturnsAsync(response.CanAccessService);

            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            context.HasSucceeded.Should().BeTrue();
            httpResponse.Verify(x => x.Redirect(It.Is<string>(c => c.Contains("/error/403/invalid-status"))));
        }

        [Test, MoqAutoData]
        public async Task Then_Succeeds_If_Provider_Ukprn_Claim_Response_Is_True(
            int ukprn,
            [Frozen] ServiceParameters serviceParameters,
            TrainingProviderAllRolesRequirement providerRequirement,
            [Frozen] Mock<ITrainingProviderAuthorizationHandler> trainingProviderAuthorizationHandler,
            TrainingProviderAllRolesAuthorizationHandler authorizationHandler)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Provider;
            var claim = new Claim(ProviderClaims.ProviderUkprn, ukprn.ToString());
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
            var context = new AuthorizationHandlerContext(new[] { providerRequirement }, claimsPrinciple, null);
            var response = new ProviderAccountResponse { CanAccessService = false };
            trainingProviderAuthorizationHandler.Setup(x => x.IsProviderAuthorized(context)).ReturnsAsync(response.CanAccessService);


            //Act
            await authorizationHandler.HandleAsync(context);

            //Assert
            Assert.IsTrue(context.HasSucceeded);
            Assert.IsFalse(context.HasFailed);
        }
    }
}
