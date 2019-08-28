﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Filters
{
    public class WhenFilteringNonEoi
    {
        [Test, MoqAutoData]
        public async Task And_Is_Provider_Then_Executes_Action(
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionContext] ActionExecutingContext context,
            Mock<ActionExecutionDelegate> mockNext,
            NonEoiNotPermittedFilterAttribute filter)
        {
            serviceParameters.AuthenticationType = AuthenticationType.Provider;

            await filter.OnActionExecutionAsync(context, mockNext.Object);

            mockNext.Verify(next => next(), Times.Once);
            context.Result.Should().Be(null);
        }

        [Test, MoqAutoData]
        public async Task And_No_EmployerId_Then_Redirect_To_Error(
            [Frozen] ServiceParameters serviceParameters,
            [ArrangeActionContext] ActionExecutingContext context,
            Mock<ActionExecutionDelegate> mockNext,
            NonEoiNotPermittedFilterAttribute filter)
        {
            serviceParameters.AuthenticationType = AuthenticationType.Employer;

            await filter.OnActionExecutionAsync(context, mockNext.Object);

            mockNext.Verify(next => next(), Times.Never());
            var result = context.Result as RedirectToRouteResult;
            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Employer_Is_Levy_Then_Executes_Action(
            [ArrangeActionContext] ActionExecutingContext context,
            string employerAccountId,
            long decodedEmployerAccountId,
            GetLegalEntitiesResponse legalEntitiesResponse,
            Mock<ActionExecutionDelegate> mockNext,
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            NonEoiNotPermittedFilterAttribute filter)
        {
            context.Result = null;
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            foreach (var accountLegalEntity in legalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = true;
                accountLegalEntity.AgreementType = AgreementType.Levy;
            }
            mockEncodingService
                .Setup(service => service.Decode(
                    context.RouteData.Values["employerAccountId"].ToString(),
                    EncodingType.AccountId))
                .Returns(decodedEmployerAccountId);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(query => query.AccountId == decodedEmployerAccountId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);

            await filter.OnActionExecutionAsync(context, mockNext.Object);

            mockNext.Verify(next => next(), Times.Once);
            context.Result.Should().Be(null);
        }

        [Test, MoqAutoData]
        public async Task And_Employer_Is_Non_Levy_And_Is_EOI_Then_Executes_Action(
            [ArrangeActionContext] ActionExecutingContext context,
            string employerAccountId,
            long decodedEmployerAccountId,
            GetLegalEntitiesResponse legalEntitiesResponse,
            Mock<ActionExecutionDelegate> mockNext,
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            NonEoiNotPermittedFilterAttribute filter)
        {
            context.Result = null;
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            foreach (var accountLegalEntity in legalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = false;
                accountLegalEntity.AgreementType = AgreementType.NonLevyExpressionOfInterest;
            }
            mockEncodingService
                .Setup(service => service.Decode(
                    context.RouteData.Values["employerAccountId"].ToString(),
                    EncodingType.AccountId))
                .Returns(decodedEmployerAccountId);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(query => query.AccountId == decodedEmployerAccountId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);

            await filter.OnActionExecutionAsync(context, mockNext.Object);

            mockNext.Verify(next => next(), Times.Once);
            context.Result.Should().Be(null);
        }

        [Test, MoqAutoData]
        public async Task And_Employer_Agreement_Not_Signed_Then_Show_EOI_Holding_View(
            [ArrangeActionContext] ActionExecutingContext context,
            string employerAccountId,
            long decodedEmployerAccountId,
            string homeUrl,
            GetLegalEntitiesResponse legalEntitiesResponse,
            Mock<ActionExecutionDelegate> mockNext,
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            NonEoiNotPermittedFilterAttribute filter)
        {
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            legalEntitiesResponse.AccountLegalEntities = new List<AccountLegalEntity>();
            mockEncodingService
                .Setup(service => service.Decode(
                    context.RouteData.Values["employerAccountId"].ToString(),
                    EncodingType.AccountId))
                .Returns(decodedEmployerAccountId);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(query => query.AccountId == decodedEmployerAccountId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Controller == "teams" &&
                        parameters.SubDomain == "accounts" &&
                        parameters.Folder == "accounts" &&
                        parameters.Id == employerAccountId)))
                .Returns(homeUrl);

            await filter.OnActionExecutionAsync(context, mockNext.Object);

            mockNext.Verify(next => next(), Times.Never());
            var result = context.Result as ViewResult;
            result.ViewName.Should().Be("NonEoiHolding");
            var model = result.Model as NonEoiHoldingViewModel;
            model.HomeLink.Should().Be(homeUrl);
        }

        [Test, MoqAutoData]
        public async Task And_Employer_Is_Non_Levy_And_Not_EOI_Then_Show_EOI_Holding_View(
            [ArrangeActionContext] ActionExecutingContext context,
            string employerAccountId,
            long decodedEmployerAccountId,
            string homeUrl,
            GetLegalEntitiesResponse legalEntitiesResponse,
            Mock<ActionExecutionDelegate> mockNext,
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            NonEoiNotPermittedFilterAttribute filter)
        {
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            foreach (var accountLegalEntity in legalEntitiesResponse.AccountLegalEntities)
            {
                accountLegalEntity.IsLevy = false;
                accountLegalEntity.AgreementType = AgreementType.Levy;
            }
            mockEncodingService
                .Setup(service => service.Decode(
                    context.RouteData.Values["employerAccountId"].ToString(),
                    EncodingType.AccountId))
                .Returns(decodedEmployerAccountId);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(query => query.AccountId == decodedEmployerAccountId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Controller == "teams" &&
                        parameters.SubDomain == "accounts" &&
                        parameters.Folder == "accounts" &&
                        parameters.Id == employerAccountId)))
                .Returns(homeUrl);

            await filter.OnActionExecutionAsync(context, mockNext.Object);

            mockNext.Verify(next => next(), Times.Never());
            var result = context.Result as ViewResult;
            result.ViewName.Should().Be("NonEoiHolding");
            var model = result.Model as NonEoiHoldingViewModel;
            model.HomeLink.Should().Be(homeUrl);
        }
    }
}