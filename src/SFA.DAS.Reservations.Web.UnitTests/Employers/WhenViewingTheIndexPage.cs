using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetNextUnreadGlobalFundingRule;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheIndexPage
    {
       
        [Test, MoqAutoData]
        public async Task ThenChecksIfRelatedUnreadRulesExists(
            string accountId,
            string expectedUserId,
            long decodedAccountId,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //Arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            mockMediator.Setup(x =>
                    x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult());
            //arrange eoi
            foreach (var accountLegalEntity in getLegalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = false;
                accountLegalEntity.AgreementType = AgreementType.NonLevyExpressionOfInterest;
            }
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(accountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            //act 
            await controller.Index(accountId);

            //assert
            mockMediator.Verify(m => m.Send(It.Is<GetNextUnreadGlobalFundingRuleQuery>(
                q => q.Id.Equals(expectedUserId, StringComparison.CurrentCultureIgnoreCase)),It.IsAny<CancellationToken>()),Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoFundingRulesExist(
            string accountId,
            string expectedUserId,
            long decodedAccountId,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetNextUnreadGlobalFundingRuleResult) null);
            //arrange eoi
            foreach (var accountLegalEntity in getLegalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = false;
                accountLegalEntity.AgreementType = AgreementType.NonLevyExpressionOfInterest;
            }
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(accountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            //act 
            var redirect = await controller.Index(accountId) as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToFundingPausedIfFundingRulesExist(
            string accountId,
            string expectedUserId,
            long decodedAccountId,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> config,
            EmployerReservationsController controller)
        {
            //Arrange
            var expectedRule = new GlobalRule
            {
                Id = 2,
                ActiveFrom = DateTime.Now.AddDays(2)
            };
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            var result = new GetNextUnreadGlobalFundingRuleResult { Rule = expectedRule };
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            //arrange eoi
            foreach (var accountLegalEntity in getLegalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = false;
                accountLegalEntity.AgreementType = AgreementType.NonLevyExpressionOfInterest;
            }
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(accountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            //act 
            var view = await controller.Index(accountId) as ViewResult;

            //assert
            Assert.IsNotNull(view);
            var viewModel = view?.Model as FundingRestrictionNotificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(view.ViewName, "FundingRestrictionNotification");
            Assert.AreEqual(expectedRule.Id, viewModel.RuleId);
            Assert.AreEqual(RuleType.GlobalRule, viewModel.TypeOfRule);
            Assert.AreEqual(expectedRule.ActiveFrom, viewModel.RestrictionStartDate);
            Assert.AreEqual(config.Object.Value.EmployerDashboardUrl, viewModel.BackLink);
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoIdFoundOnNextGlobalFundingRule(
            string accountId,
            string expectedUserId,
            long decodedAccountId,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{ActiveFrom = DateTime.Now}});
            //arrange eoi
            foreach (var accountLegalEntity in getLegalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = false;
                accountLegalEntity.AgreementType = AgreementType.NonLevyExpressionOfInterest;
            }
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(accountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            //act 
            var redirect = await controller.Index(accountId) as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }

        [Test, MoqAutoData]
        public async Task ThenRedirectToStartIfNoActiveFromDateFoundOnNextGlobalFundingRule(
            string accountId,
            string expectedUserId,
            long decodedAccountId,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            mockMediator.Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{Id = 2}});
            //arrange eoi
            foreach (var accountLegalEntity in getLegalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = false;
                accountLegalEntity.AgreementType = AgreementType.NonLevyExpressionOfInterest;
            }
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(accountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            //act 
            var redirect = await controller.Index(accountId) as RedirectToActionResult;

            //assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(redirect.ActionName, "Start");
        }

        [Test, MoqAutoData]
        public async Task And_Not_Eoi_And_NonLevy_Then_Returns_NonEoiHolding_View(
            string accountId,
            string expectedUserId,
            string homeLink,
            long decodedAccountId,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            
            mockMediator
                .Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{Id = 2}});
            //arrange eoi
            foreach (var accountLegalEntity in getLegalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = false;
                accountLegalEntity.AgreementType = AgreementType.Levy;
            }
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(accountId, EncodingType.AccountId))
                .Returns(decodedAccountId);
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(It.Is<UrlParameters>(parameters => 
                    parameters.Controller == "teams" &&
                    parameters.SubDomain == "accounts" &&
                    parameters.Folder == "accounts" &&
                    parameters.Id == accountId
                )))
                .Returns(homeLink);

            //act 
            var result = await controller.Index(accountId) as ViewResult;

            //assert
            Assert.AreEqual("NonEoiHolding", result.ViewName);
            var model = result.Model as NonEoiHoldingViewModel;
            Assert.AreEqual(homeLink, model.BackLink);
            Assert.AreEqual(homeLink, model.HomeLink);
        }

        [Test, MoqAutoData]
        // no agreement - no legal entities returned from api
        public async Task And_No_Agreement_Signed_Then_Returns_NonEoiHolding_View(
            string accountId,
            string expectedUserId,
            string homeLink,
            long decodedAccountId,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //arrange
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId)
            }));
            
            mockMediator
                .Setup(x => x.Send(It.IsAny<GetNextUnreadGlobalFundingRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetNextUnreadGlobalFundingRuleResult {Rule = new GlobalRule{Id = 2}});
            //arrange eoi
            getLegalEntitiesResponse.AccountLegalEntities = new List<AccountLegalEntity>();
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(accountId, EncodingType.AccountId))
                .Returns(decodedAccountId);
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(It.Is<UrlParameters>(parameters => 
                    parameters.Controller == "teams" &&
                    parameters.SubDomain == "accounts" &&
                    parameters.Folder == "accounts" &&
                    parameters.Id == accountId
                )))
                .Returns(homeLink);

            //act 
            var result = await controller.Index(accountId) as ViewResult;

            //assert
            Assert.AreEqual("NonEoiHolding", result.ViewName);
            var model = result.Model as NonEoiHoldingViewModel;
            Assert.AreEqual(homeLink, model.BackLink);
            Assert.AreEqual(homeLink, model.HomeLink);
        }
    }
}
