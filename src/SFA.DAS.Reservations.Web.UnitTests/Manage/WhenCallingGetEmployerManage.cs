using System;
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
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Manage
{
    [TestFixture]
    public class WhenCallingGetEmployerManage
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_List_Of_Reservations_For_Single_Employer_Account(
            ReservationsRouteModel routeModel,
            GetReservationsResult getReservationsResult,
            long decodedAccountId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getReservationsResult);
            mockEncodingService
                .Setup(service => service.Decode(routeModel.EmployerAccountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            await controller.EmployerManage(routeModel);
            
            mockMediator.Verify(mediator =>
                    mediator.Send(
                        It.Is<GetReservationsQuery>(query => query.AccountId == decodedAccountId),
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_List_Of_Reservations_For_Single_Employer_Account(
            ReservationsRouteModel routeModel,
            GetReservationsResult getReservationsResult,
            string hashedId,
            string expectedUrl,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IExternalUrlHelper> mockExternalUrlHelper,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getReservationsResult);
           
            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);

            mockExternalUrlHelper
                .Setup(h => h.GenerateAddApprenticeUrl(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<uint?>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()))
                .Returns(expectedUrl);

            getReservationsResult.Reservations.ToList().ForEach(c =>
            {
                c.Status = ReservationStatus.Pending;
                c.IsExpired = false;
            });

            var expectedReservations = new List<ReservationViewModel>();
            expectedReservations.AddRange(
                getReservationsResult.Reservations.Select(
                    reservation => new ReservationViewModel(reservation, expectedUrl, routeModel.UkPrn)));

            var result = await controller.EmployerManage(routeModel) as ViewResult;

            result.Should().NotBeNull();
            result.ViewName.Should().Be(ViewNames.EmployerManage);
            var viewModel = result.Model as ManageViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Reservations.Should().BeEquivalentTo(expectedReservations,
                options => options.ExcludingMissingMembers());
        }

        [Test, MoqAutoData]
        public async Task Add_Apprentice_Url_UkPrn_Will_Be_Populated_From_RouteModel_Reservation(
            ReservationsRouteModel routeModel,
            GetTrustedEmployersResponse getTrustedEmployersResponse,
            Reservation reservation,
            string hashedId,
            string expectedUrl,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getTrustedEmployersResponse);

            reservation.ProviderId = null;
            reservation.IsExpired = false;

            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetReservationsResult
                {
                    Reservations = new []{ reservation }
                });

            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);

            mockUrlHelper.Setup(h => h.GenerateAddApprenticeUrl(
                reservation.Id,
                hashedId,
                reservation.Course.Id,
                routeModel.UkPrn,
                reservation.StartDate,
                routeModel.CohortReference,
                routeModel.EmployerAccountId,
                false,
                ""))
                .Returns(expectedUrl);
            
            var result = await controller.EmployerManage(routeModel) as ViewResult;
            
            var viewModel = result?.Model as ManageViewModel;
            viewModel.Should().NotBeNull();

            Assert.IsTrue(viewModel.Reservations.All(c=>c.ApprenticeUrl.Equals(expectedUrl)));
        }
    }
}