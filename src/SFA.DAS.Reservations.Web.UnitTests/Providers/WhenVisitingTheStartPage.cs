﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetFundingRules;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenVisitingTheStartPage
    {
        private ProviderReservationsController _controller;
        private Mock<IMediator> _mediator;
        private const uint ukPrn = 1234;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization {ConfigureMembers = true});
            _mediator = fixture.Freeze<Mock<IMediator>>();
            _mediator.Setup(m => m.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    AccountRules = new List<ReservationRule>(),
                    GlobalRules = new List<GlobalRule>()
                });
            _mediator.Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse
                {
                    Employers = new List<Employer> { new Employer() }
                });

            _controller = fixture.Create<ProviderReservationsController>();
        }

        [Test]
        public async Task ThenWillBeRoutedToProviderStartPage()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    AccountRules = new List<ReservationRule>(),
                    GlobalRules = new List<GlobalRule>()
                });

            //Act
            var result = await _controller.Start(123, true) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
            var model = result.Model as ProviderStartViewModel;
            Assert.AreEqual(true, model.IsFromManage);
        }

        [Test]
        public async Task ThenWillBeRedirectedToFundingStoppedPageIfGlobalRuleExists()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<GetFundingRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFundingRulesResult
                {
                    AccountRules = new List<ReservationRule>(),
                    GlobalRules = new List<GlobalRule>
                    {
                        new GlobalRule{ActiveFrom = DateTime.Now.AddDays(-2)}
                    }
                });

            //Act
            var result = await _controller.Start(123, true) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ProviderFundingPaused", result.ViewName);
        }

        [Test]
        public async Task Then_The_No_Permissions_View_Is_Shown_If_You_Have_No_Employers_To_Act_On_Behalf_Of()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse
                {
                    Employers = new List<Employer> ()
                });

            //Act
            var result = await _controller.Start(123, true) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("NoPermissions", result.ViewName);
        }

        [Test, MoqAutoData]
        public async Task WhenFundingIsPaused_AndFromManage_ThenBackLinkSetToManage(
            [Frozen] Mock<IMediator> mockMediator,
            GetFundingRulesResult result,
            [Frozen] Mock<IUrlHelper> mockUrlHelper,
            [Frozen] Mock<IExternalUrlHelper> mockExternalUrlHelper,
            GlobalRule rule,
            ProviderReservationsController controller)
        {
            //Arrange
            var expectedBacklink = "Test12345";
            var expectedViewnName = "ProviderFundingPaused";
            rule.ActiveFrom = DateTime.UtcNow.AddDays(-5);
            result.GlobalRules.Add(rule);
            
            mockMediator
                .Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), CancellationToken.None))
                .ReturnsAsync(result);
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(expectedBacklink);
            mockExternalUrlHelper
                .Setup(x => x.GenerateDashboardUrl(It.IsAny<string>()))
                .Returns("unexpectedUrl");
            controller.Url = mockUrlHelper.Object;
            //Act
            var viewResult = await controller.Start(ukPrn, true) as ViewResult;

            //Assert
            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult.Model as string);
            Assert.AreEqual(expectedBacklink, viewResult.Model as string);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(y => y.RouteName == RouteNames.ProviderManage)),Times.Once);
            mockExternalUrlHelper.Verify(x => x.GenerateDashboardUrl(It.IsAny<string>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task WhenFundingIsPaused_AndNotFromManage_ThenBackLinkSetToDashBoard(
            [Frozen] Mock<IMediator> mockMediator,
            GetFundingRulesResult result,
            [Frozen] Mock<IUrlHelper> mockUrlHelper,
            [Frozen] Mock<IExternalUrlHelper> mockExternalUrlHelper,
            GlobalRule rule,
            ProviderReservationsController controller)
        {
            //Arrange
            var expectedBacklink = "Test12345";
            var expectedViewnName = "ProviderFundingPaused";
            rule.ActiveFrom = DateTime.UtcNow.AddDays(-5);
            result.GlobalRules.Add(rule);

            mockMediator
                .Setup(x => x.Send(It.IsAny<GetFundingRulesQuery>(), CancellationToken.None))
                .ReturnsAsync(result);
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns("unexpectedBacklink");
            mockExternalUrlHelper
                .Setup(x => x.GenerateDashboardUrl(It.IsAny<string>()))
                .Returns(expectedBacklink);
            controller.Url = mockUrlHelper.Object;

            //Act
            var viewResult = await controller.Start(ukPrn, false) as ViewResult;

            //Assert
            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult.Model as string);
            Assert.AreEqual(expectedBacklink, viewResult.Model as string);
            mockExternalUrlHelper.Verify(x => x.GenerateDashboardUrl(It.IsAny<string>()), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(y => y.RouteName == RouteNames.ProviderManage)), Times.Never);
        }
    }
}
