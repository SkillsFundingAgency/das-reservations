using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextActiveGlobalFundingRule;
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
        public async Task ThenWillShowEmployerRestrictionNotificationIfGlobalRuleIsFound(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            //arrange
            routeModel.UkPrn = null;

            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextActiveGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextActiveGlobalFundingRuleResult
                {
                    Rule = new GlobalRule(){ActiveFrom = DateTime.Now}
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
            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextActiveGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextActiveGlobalFundingRuleResult
                {
                    Rule = new GlobalRule {ActiveFrom = DateTime.Now}
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
        public async Task ThenWillRedirtectToCreateReservationPageIfGlobalRuleNotFound(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            //arrange
            mockMediator.Setup(m => m.Send(
                    It.IsAny<GetNextActiveGlobalFundingRuleQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetNextActiveGlobalFundingRuleResult());

            //act
            var redirect = await controller.CreateReservation(routeModel) as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual("EmployerReservations", redirect.ControllerName);
            Assert.AreEqual("Start", redirect.ActionName);
        }
    }
}