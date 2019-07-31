using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostSelectReservation
    {
        [Test, MoqAutoData]
        public async Task And_Has_UkPrn_And_New_Reservation_Then_Redirects_To_ProviderApprenticeshipTraining_And_The_Cached_Reservation_Is_Created(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            viewModel.SelectedReservationId = Guid.Parse(Guid.Empty.ToString().Replace("0","9"));
            routeModel.Id = Guid.Empty;
           
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = matchedEmployer.AccountId,
                        AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = matchedEmployer.AccountLegalEntityId,
                        AccountLegalEntityName = matchedEmployer.AccountLegalEntityName,
                        AccountName = matchedEmployer.AccountName,
                        CohortRef = viewModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });


            var result = await controller.PostSelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result?.RouteName.Should().NotBeNull();
            result.RouteName.Should().Be(RouteNames.ProviderApprenticeshipTraining);
            result.RouteValues["ukPrn"].Should().Be(routeModel.UkPrn);
            result.RouteValues["id"].Should().NotBe(Guid.Empty);
            mockMediator.Verify(x =>
                x.Send(It.Is<CacheReservationEmployerCommand>(
                        c =>
                            c.CohortRef.Equals(viewModel.CohortReference) &&
                            c.AccountId.Equals(matchedEmployer.AccountId) &&
                            c.AccountLegalEntityName.Equals(matchedEmployer.AccountLegalEntityName) &&
                            c.AccountLegalEntityPublicHashedId.Equals(matchedEmployer.AccountLegalEntityPublicHashedId) &&
                            c.AccountName.Equals(matchedEmployer.AccountName) &&
                            c.UkPrn.Equals(routeModel.UkPrn) &&
                            !c.Id.Equals(Guid.Empty) &&
                            c.AccountLegalEntityId.Equals(matchedEmployer.AccountLegalEntityId))
                    , It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Reservation_Limit_Has_Been_Reached_Then_ReservationLimit_Reached_View_Is_Shown_With_The_Back_Link_Going_To_Cohort_Details(
                ReservationsRouteModel routeModel,
                SelectReservationViewModel viewModel,
                GetTrustedEmployersResponse employersResponse,
                string cohortDetailsUrl,
                [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
                [Frozen] Mock<IMediator> mockMediator,
                ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            viewModel.SelectedReservationId = Guid.Parse(Guid.Empty.ToString().Replace("0", "9"));
            routeModel.Id = Guid.Empty;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ReservationLimitReachedException(viewModel.AccountId));
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Id == routeModel.UkPrn.ToString() &&
                        parameters.Controller == $"apprentices/{viewModel.CohortReference}" &&
                        parameters.Action == "details" )))
                .Returns(cohortDetailsUrl);

            //Act
            var result = await controller.PostSelectReservation(routeModel, viewModel) as ViewResult;
                
            //Assert
            result.ViewName.Should().Be("ReservationLimitReached");
            result.Model.Should().Be(cohortDetailsUrl);
        }

        [Test, MoqAutoData]
        public async Task And_Provider_Has_No_Create_Permission_Then_NoPermissions_View_Is_Shown_With_The_Back_Link_Going_To_Cohort_Details(
                ReservationsRouteModel routeModel,
                SelectReservationViewModel viewModel,
                GetTrustedEmployersResponse employersResponse,
                string cohortDetailsUrl,
                [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
                [Frozen] Mock<IMediator> mockMediator,
                ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            viewModel.SelectedReservationId = Guid.Parse(Guid.Empty.ToString().Replace("0", "9"));
            routeModel.Id = Guid.Empty;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProviderNotAuthorisedException(viewModel.AccountId,routeModel.UkPrn.Value));
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Id == routeModel.UkPrn.ToString() &&
                        parameters.Controller == $"apprentices/{viewModel.CohortReference}" &&
                        parameters.Action == "details")))
                .Returns(cohortDetailsUrl);

            //Act
            var result = await controller.PostSelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            result.ViewName.Should().Be("NoPermissions");
            result.Model.Should().Be(cohortDetailsUrl);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_ReservationId_Then_Redirects_To_AddApprentice_With_Reservation_Details(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetReservationResult reservationResult,
            string addApprenticeUrl,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            ReservationsController controller)
        {
            mockMediator.Setup(x => x.Send(It.Is<GetReservationQuery>(c => c.Id.Equals(viewModel.SelectedReservationId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationResult);

            mockUrlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(
                    viewModel.SelectedReservationId.Value,
                    routeModel.AccountLegalEntityPublicHashedId,
                    reservationResult.Course.Id,
                    routeModel.UkPrn,
                    reservationResult.StartDate,
                    viewModel.CohortReference,
                    routeModel.EmployerAccountId))
                .Returns(addApprenticeUrl);
            
            var result = await controller.PostSelectReservation(routeModel, viewModel) as RedirectResult;

            result.Url.Should().Be(addApprenticeUrl);
        }

        [Test, MoqAutoData]
        public async Task And_No_Option_Has_Been_Selected_Then_The_Validation_Errors_Are_Returned_To_The_User_And_ViewModel_Recreated(
            ReservationsRouteModel routeModel,
            GetAvailableReservationsResult availableReservationsResult,
            SelectReservationViewModel viewModel,
            string cohortDetailsUrl,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            viewModel.SelectedReservationId = Guid.Empty;
            mockMediator
                .Setup(x => x.Send(It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(viewModel.AccountId)),
                    It.IsAny<CancellationToken>())).ReturnsAsync(availableReservationsResult);
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Id == routeModel.UkPrn.ToString() &&
                        parameters.Controller == $"apprentices/{viewModel.CohortReference}" &&
                        parameters.Action == "details")))
                .Returns(cohortDetailsUrl);

            var result = await controller.PostSelectReservation(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be("ProviderSelect");
            var actualModel = result.Model as SelectReservationViewModel;
            actualModel.Should().NotBeNull();
            actualModel.AvailableReservations.Should().BeEquivalentTo(availableReservationsResult.Reservations.Select(c=>new AvailableReservationViewModel(c)));
        }
    }
}