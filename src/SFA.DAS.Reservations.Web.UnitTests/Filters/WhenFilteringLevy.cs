using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Filters
{
    public class WhenFilteringLevy
    {
        [Test, MoqAutoData]
        public async Task AndIsProvider_ThenContinuesToAction(
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            LevyNotPermittedFilter filter)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Provider;
            
            //Act
           await filter.OnActionExecutionAsync(context, nextMethod.Object);

           //Assert
           nextMethod.Verify( x => x(), Times.Once);
           Assert.Null(context.Result);
        }

        [Test, MoqAutoData]
        public async Task AndIsANonLevyEmployer_ThenContinuesToAction(
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [ArrangeActionContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            LevyNotPermittedFilter filter)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            legalEntitiesResponse.AccountLegalEntities = legalEntities;
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            foreach (var legalEntity in legalEntitiesResponse.AccountLegalEntities) { legalEntity.IsLevy = false;}
            
            mockEncodingService
                .Setup(x => x.TryDecode(employerAccountId, EncodingType.AccountId, out decodedId))
                .Returns(true);
            
            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntitiesQuery>(y => y.AccountId == decodedId),It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);

            //Act
            await filter.OnActionExecutionAsync(context, nextMethod.Object);

            //Assert
            Assert.Null(context.Result);
            nextMethod.Verify(x => x(), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task AndIsALevyEmployer_ThenRedirectsToAccessDeniedPage(
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [ArrangeActionContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            LevyNotPermittedFilter filter)
        {
            //Arrange
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            legalEntitiesResponse.AccountLegalEntities = legalEntities;
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            foreach (var legalEntity in legalEntitiesResponse.AccountLegalEntities) { legalEntity.IsLevy = true; }
            mockEncodingService
                .Setup(x => x.TryDecode(employerAccountId, EncodingType.AccountId, out decodedId))
                .Returns(true);
            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntitiesQuery>(y => y.AccountId == decodedId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);

            //Act
            await filter.OnActionExecutionAsync(context, nextMethod.Object);

            //Assert
            Assert.NotNull(context.Result);
            Assert.True(context.Result is RedirectToRouteResult);
            Assert.AreEqual((context.Result as RedirectToRouteResult).RouteName, RouteNames.Error403);
            nextMethod.Verify(x => x(), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task AndIsSuspended_ThenRedirectsToAccountSuspended(
            [Frozen] Mock<IConfiguration> configuration,
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionContext] ActionExecutingContext context,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            LevyNotPermittedFilter filter)
        {
            //Arrange
            configuration.Setup(x => x["ResourceEnvironmentName"]).Returns("int");
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            
            context.HttpContext = new DefaultHttpContext();
            context.HttpContext.Response.Body = new MemoryStream();
            
            
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {new Claim(ClaimTypes.AuthorizationDecision, "Suspended")}));
            
            
            //Act
            await filter.OnActionExecutionAsync(context, nextMethod.Object);
            
            //Assert
            nextMethod.Verify(x => x(), Times.Never);
            Assert.AreEqual((int)HttpStatusCode.Redirect, context.HttpContext.Response.StatusCode);
            Assert.AreEqual("https://employerprofiles.int-eas.apprenticeships.education.gov.uk/service/account-unavailable", context.HttpContext.Response.Headers["Location"].ToString());
            
        }
    }
}
