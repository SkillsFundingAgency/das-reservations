using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetCompleted
    {
        [Test, MoqAutoData]
        public async Task Then_It_Calls_Mediator_To_Get_Reservation(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            await controller.Completed(routeModel);

            mockMediator.Verify(mediator => mediator.Send(It.Is<GetReservationQuery>(query => query.Id == routeModel.Id), CancellationToken.None));
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_ViewModel(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> mockConfig,
            [NoAutoProperties] ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            var result = await controller.Completed(routeModel);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<CompletedViewModel>().Subject;

            viewResult.ViewName.Should().Be(ViewNames.ProviderCompleted);

            model.AccountLegalEntityName.Should().Be(mediatorResult.AccountLegalEntityName);
            model.TrainingDateDescription.Should().Be(new TrainingDateModel
            {
                StartDate = mediatorResult.StartDate,
                EndDate = mediatorResult.ExpiryDate
            }.GetGDSDateString());
            model.CourseDescription.Should().Be(mediatorResult.Course.CourseDescription);
            model.StartDate.Should().Be(mediatorResult.StartDate);
            model.CourseId.Should().Be(mediatorResult.Course.Id);
            model.UkPrn.Should().Be(mediatorResult.UkPrn);
            model.CohortRef.Should().Be(routeModel.CohortReference);
            model.JourneyData.Should().Be(routeModel.JourneyData);
        }

        [Test, MoqAutoData]
        public async Task And_No_UkPrn_Then_It_Uses_Employer_View_And_Uses_Provider_Id_If_Not_Null(
            ReservationsRouteModel routeModel,
            GetReservationResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
            [NoAutoProperties] ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mediatorResult.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationQuery>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            var result = await controller.Completed(routeModel);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be(ViewNames.EmployerCompleted);
            var model = viewResult.Model.Should().BeOfType<CompletedViewModel>().Subject;
            model.UkPrn.Should().Be(routeModel.ProviderId);
        }

        [Test, MoqAutoData]
        public void Then_If_No_ReservationId_Given_An_Error_Is_Throw(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] ReservationsController controller)
        {
            routeModel.Id = null;

            Assert.ThrowsAsync<ArgumentException>(() => controller.Completed(routeModel));
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_ValidationError_Then_Return_Provider_Completed_View(
            ReservationsRouteModel routeModel,
             [Frozen] Mock<IMediator> mockMediator,
             GetReservationResult result,
            [NoAutoProperties] ReservationsController controller)
        {
            routeModel.Id = Guid.NewGuid();
            routeModel.UkPrn = null;

            mockMediator.Setup(t => t.Send(It.IsAny<GetReservationQuery>(), It.IsAny<CancellationToken>())).
                ReturnsAsync(result);

            var actual = await controller.Completed(routeModel) as ViewResult;

            actual.ViewName.Should().Be(ViewNames.EmployerCompleted);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Redirects_To_Recruit_Url(
         ReservationsRouteModel routeModel,
         [Frozen] Mock<IMediator> mockMediator,
         [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
         [NoAutoProperties] ReservationsController controller)
        {
            routeModel.Id = Guid.NewGuid();
            routeModel.UkPrn = 123456;
            routeModel.UseLearnerData = true;
            routeModel.EmployerAccountId = "1";

            var result = new GetReservationResult()
            {
                UkPrn = routeModel.UkPrn,
                AccountLegalEntityName = "Test",
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                Course = new Domain.Courses.Course("1", "course1", 1),
            };

            var expectedUrl = $"https://recruit/{routeModel.UkPrn}";

            mockUrlHelper
             .Setup(h => h.GenerateUrl(
                 It.Is<Domain.Interfaces.UrlParameters>(p =>
                     p.Id == routeModel.UkPrn.ToString() && p.SubDomain == "recruit")))
             .Returns(expectedUrl);

            mockMediator.Setup(t => t.Send(It.IsAny<GetReservationQuery>(),
                It.IsAny<CancellationToken>())).
                ReturnsAsync(result);

            var actual = await controller.Completed(routeModel) as ViewResult;

            var model = actual.Model.Should().BeOfType<CompletedViewModel>().Subject;
            model.RecruitUrl.Should().Be(expectedUrl);

            mockUrlHelper.Verify(x => x.GenerateUrl(It.Is<Domain.Interfaces.UrlParameters>(p => p.SubDomain == "recruit" && p.Id == routeModel.UkPrn.ToString())), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Redirects_To_HomePage_Url(
         ReservationsRouteModel routeModel,
         [Frozen] Mock<IMediator> mockMediator,
         [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
         [NoAutoProperties] ReservationsController controller)
        {
            routeModel.Id = Guid.NewGuid();
            routeModel.UkPrn = 123456;
            routeModel.UseLearnerData = true;
            routeModel.EmployerAccountId = "1";

            var result = new GetReservationResult()
            {
                UkPrn = routeModel.UkPrn,
                AccountLegalEntityName = "Test",
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                Course = new Domain.Courses.Course("1", "course1", 1),
            };

            var expectedUrl = $"https://dashboard/home";

            mockUrlHelper
             .Setup(h => h.GenerateDashboardUrl(It.Is<string>(y => y == routeModel.EmployerAccountId)))
             .Returns(expectedUrl);

            mockMediator.Setup(t => t.Send(It.IsAny<GetReservationQuery>(),
                It.IsAny<CancellationToken>())).
                ReturnsAsync(result);

            var actual = await controller.Completed(routeModel) as ViewResult;

            var model = actual.Model.Should().BeOfType<CompletedViewModel>().Subject;
            model.HomepageUrl.Should().Be(expectedUrl);
            mockUrlHelper.Verify(x => x.GenerateDashboardUrl(It.Is<string>(p => p == routeModel.EmployerAccountId)), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Redirects_To_AddApprentice_Url(
         ReservationsRouteModel routeModel,
         [Frozen] Mock<IMediator> mockMediator,
         [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
         [NoAutoProperties] ReservationsController controller)
        {
            routeModel.Id = Guid.NewGuid();
            routeModel.UkPrn = 123456;
            routeModel.UseLearnerData = true;
            routeModel.EmployerAccountId = "1";

            var result = new GetReservationResult()
            {
                UkPrn = routeModel.UkPrn,
                AccountLegalEntityName = "Test",
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                Course = new Domain.Courses.Course("1", "course1", 1),
            };

            var expectedUrl = $"https://apprentice/add";

            mockUrlHelper
             .Setup(h => h.GenerateAddApprenticeUrl(
                 routeModel.Id.Value,
                 routeModel.AccountLegalEntityPublicHashedId,
                 result.Course.Id,
                 result.UkPrn, result.StartDate,
                 routeModel.CohortReference,
                 routeModel.EmployerAccountId,
                 It.IsAny<bool>(),
                 It.IsAny<string>(),
                 It.IsAny<string>(),
                 routeModel.JourneyData,
                 It.IsAny<Guid?>(),
                 It.IsAny<bool?>(),
                 routeModel.UseLearnerData
                 )).Returns(expectedUrl);

            mockMediator.Setup(t => t.Send(It.IsAny<GetReservationQuery>(),
                It.IsAny<CancellationToken>())).
                ReturnsAsync(result);

            var actual = await controller.Completed(routeModel) as ViewResult;

            var model = actual.Model.Should().BeOfType<CompletedViewModel>().Subject;
            model.AddAnApprenticeUrl.Should().Be(expectedUrl);

            mockUrlHelper
            .Verify(h => h.GenerateAddApprenticeUrl(
                routeModel.Id.Value,
                routeModel.AccountLegalEntityPublicHashedId,
                result.Course.Id,
                result.UkPrn, result.StartDate,
                routeModel.CohortReference,
                routeModel.EmployerAccountId,
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                routeModel.JourneyData,
                It.IsAny<Guid?>(),
                It.IsAny<bool?>(),
                routeModel.UseLearnerData
                ), Times.Once);
        }
    }
}