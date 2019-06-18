using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    [SuppressMessage("ReSharper", "NUnit.MethodWithParametersAndTestAttribute")]
    public class WhenCreatingReservation
    {
        [Test, MoqAutoData]
        public async Task ThenWillGetAnyProviderGlobalRulesUserHasNotRead(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            //arrange
            var expectedUkPrnId = "555";
            var claim = new Claim(ProviderClaims.ProviderUkprn, expectedUkPrnId);

            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim })); ;

            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextUnreadGlobalFundingRuleResult
                {
                    Rule = new GlobalRule{ActiveFrom = DateTime.Now}
                });
            
            //act
            await controller.CreateReservation(routeModel);
          
            //assert
            mockMediator.Verify(m => m.Send(
                It.Is<GetNextUnreadGlobalFundingRuleQuery>(query => query.Id.Equals(expectedUkPrnId)),
                It.IsAny<CancellationToken>()),Times.Once());

        }

        [Test, MoqAutoData]
        public async Task ThenWillGetAnyEmployerGlobalRulesUserHasNotRead(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            //arrange
            var expectedUserId = "12564";
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId);

            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));

            routeModel.UkPrn = null;

            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextUnreadGlobalFundingRuleResult
                {
                    Rule = new GlobalRule{ActiveFrom = DateTime.Now}
                });
            
            //act
            await controller.CreateReservation(routeModel);
          
            //assert
            mockMediator.Verify(m => m.Send(
                It.Is<GetNextUnreadGlobalFundingRuleQuery>(query => query.Id.Equals(expectedUserId)),
                It.IsAny<CancellationToken>()),Times.Once());

        }

        [Test, MoqAutoData]
        public async Task ThenWillShowEmployerRestrictionNotificationIfGlobalRuleIsFound(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            //arrange
            routeModel.UkPrn = null;
            
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, "123");

            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {claim}));

            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextUnreadGlobalFundingRuleResult
                {
                    Rule = new GlobalRule{Id = 2, ActiveFrom = DateTime.Now}
                });

            //act
            var view = await controller.CreateReservation(routeModel) as ViewResult;
            var viewModel = view?.Model as FundingRestrictionNotificationViewModel;

            //assert
            Assert.IsNotNull(view);
            Assert.IsNotNull(viewModel);
            Assert.AreEqual("../EmployerReservations/FundingRestrictionNotification", view.ViewName);
            Assert.AreEqual(RouteNames.EmployerManage, viewModel.BackLink);
        }

        [Test, MoqAutoData]
        public async Task ThenWillShowProviderRestrictionNotificationIfGlobalRuleIsFound(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            //arrange
            routeModel.UkPrn = 120;
            
            var claim = new Claim(ProviderClaims.ProviderUkprn, "555");

            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));

            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextUnreadGlobalFundingRuleResult
                {
                    Rule = new GlobalRule {Id = 2, ActiveFrom = DateTime.Now}
                });

            //act
            var view = await controller.CreateReservation(routeModel) as ViewResult;
            var viewModel = view?.Model as FundingRestrictionNotificationViewModel;

            //assert
            Assert.IsNotNull(view);
            Assert.IsNotNull(viewModel);
            Assert.AreEqual("../ProviderReservations/FundingRestrictionNotification", view.ViewName);
            Assert.AreEqual(RouteNames.ProviderManage, viewModel.BackLink);
        }

        [Test, MoqAutoData]
        public async Task ThenWillRedirectToEmployerCreateReservationPageIfGlobalRuleNotFound(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            //arrange   
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());

            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {claim}));

            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextUnreadGlobalFundingRuleResult());

            //act
            var redirect = await controller.CreateReservation(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.EmployerStart, redirect.RouteName);
        }

        [Test, MoqAutoData]
        public async Task ThenWillRedirtectToProviderCreateReservationPageIfGlobalRuleNotFound(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            //arrange           
            var claim = new Claim(ProviderClaims.ProviderUkprn, "444");

            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));

            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextUnreadGlobalFundingRuleResult());

            //act
            var redirect = await controller.CreateReservation(routeModel) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.ProviderStart, redirect.RouteName);
        }
    }
}