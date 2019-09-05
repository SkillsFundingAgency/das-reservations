using System.Collections.Generic;
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
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetApprenticeshipTraining
    {
        [Test, MoqAutoData]
        public async Task Then_It_Calls_Start_Date_Service_To_Get_Start_Dates(
            ReservationsRouteModel routeModel,
            GetCachedReservationResult cachedReservationResult,
            long accountLegalEntityId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ITrainingDateService> mockStartDateService,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            mockEncodingService
                .Setup(service => service.Decode(
                    cachedReservationResult.AccountLegalEntityPublicHashedId,
                    EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);

            await controller.ApprenticeshipTraining(routeModel);

            mockStartDateService.Verify(provider => provider.GetTrainingDates(accountLegalEntityId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_Apprenticeship_Training_View_With_Mapped_Values(
            ReservationsRouteModel routeModel,
            IEnumerable<TrainingDateModel> expectedStartDates,
            GetCoursesResult getCoursesResult,
            GetCachedReservationResult cachedReservationResult,
            long accountLegalEntityId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ITrainingDateService> mockStartDateService,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            cachedReservationResult.CohortRef = string.Empty;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getCoursesResult);
            mockEncodingService
                .Setup(service => service.Decode(
                    cachedReservationResult.AccountLegalEntityPublicHashedId,
                    EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityId);
            mockStartDateService
                .Setup(service => service.GetTrainingDates(accountLegalEntityId))
                .ReturnsAsync(expectedStartDates);
            var mappedDates = expectedStartDates.Select(startDateModel => new TrainingDateViewModel(startDateModel)).OrderBy(model => model.StartDate);
            var mappedCourses = getCoursesResult.Courses.Select(course => new CourseViewModel(course));
            routeModel.FromReview = false;

            var result = await controller.ApprenticeshipTraining(routeModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ApprenticeshipTrainingViewModel>()
                .Subject;
            
            viewModel.PossibleStartDates.Should().BeEquivalentTo(mappedDates);
            viewModel.Courses.Should().BeEquivalentTo(mappedCourses);
            viewModel.CourseId.Should().Be(cachedReservationResult.CourseId);
            viewModel.IsProvider.Should().BeTrue();
            viewModel.RouteName.Should().Be(RouteNames.ProviderCreateApprenticeshipTraining);
            viewModel.BackLink.Should().Be(RouteNames.ProviderConfirmEmployer);
            viewModel.AccountLegalEntityPublicHashedId.Should()
                .Be(cachedReservationResult.AccountLegalEntityPublicHashedId);
        }

        [Test, MoqAutoData]
        public async Task Then_It_Sets_The_Back_Link_Correctly_Coming_From_The_Review_Screen(
            ReservationsRouteModel routeModel,
            GetCoursesResult getCoursesResult,
            GetCachedReservationResult cachedReservationResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            cachedReservationResult.CohortRef = string.Empty;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getCoursesResult);
            routeModel.FromReview = true;

            var result = await controller.ApprenticeshipTraining(routeModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ApprenticeshipTrainingViewModel>()
                .Subject;
            viewModel.BackLink.Should().Be(RouteNames.ProviderReview);
        }


        [Test, MoqAutoData]
        public async Task Then_It_Sets_The_Back_Link_Correctly_Coming_From_The_SelectReservation_Screen(
            ReservationsRouteModel routeModel,
            GetCoursesResult getCoursesResult,
            GetCachedReservationResult cachedReservationResult,
            string cohortDetailsUrl,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            cachedReservationResult.CohortRef = "ABC123";
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedReservationResult);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getCoursesResult);
            routeModel.FromReview = true;
            mockUrlHelper
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn,routeModel.EmployerAccountId,cachedReservationResult.CohortRef))
                .Returns(cohortDetailsUrl);

            var result = await controller.ApprenticeshipTraining(routeModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ApprenticeshipTrainingViewModel>()
                .Subject;
            viewModel.CohortRef.Should().Be(cachedReservationResult.CohortRef);
            viewModel.BackLink.Should().Be(cohortDetailsUrl);
        }




        [Test, MoqAutoData]
        public async Task And_Has_Previous_Reservation_Then_Loads_Existing_Reservation_To_ViewModel(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {          
            await controller.ApprenticeshipTraining(routeModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetCachedReservationQuery>(query => query.Id == routeModel.Id), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Employer_Then_IsProvider_Is_False(
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;

            var result = await controller.ApprenticeshipTraining(routeModel) as ViewResult;

            ((ApprenticeshipTrainingViewModel) result.Model).IsProvider.Should().BeFalse();
        }
    }
}