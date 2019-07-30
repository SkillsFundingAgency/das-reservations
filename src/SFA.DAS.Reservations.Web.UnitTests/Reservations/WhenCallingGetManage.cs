using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    [SuppressMessage("ReSharper", "NUnit.MethodWithParametersAndTestAttribute")]
    public class WhenCallingGetManage
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Gets_List_Of_Reservations_For_All_Trusted_Employer_Accounts(
            ReservationsRouteModel routeModel,
            GetTrustedEmployersResponse getTrustedEmployersResponse,
            GetReservationsResult getReservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getTrustedEmployersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getReservationsResult);

            await controller.Manage(routeModel);

            mockMediator.Verify(mediator =>
                    mediator.Send(
                        It.Is<GetTrustedEmployersQuery>(query => query.UkPrn == routeModel.UkPrn),
                        It.IsAny<CancellationToken>()),
                Times.Once);

            foreach (var employer in getTrustedEmployersResponse.Employers)
            {
                mockMediator.Verify(mediator =>
                        mediator.Send(
                            It.Is<GetReservationsQuery>(query => query.AccountId == employer.AccountId),
                            It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Returns_List_Of_Reservations_For_All_Trusted_Employer_Accounts(
            ReservationsRouteModel routeModel,
            GetTrustedEmployersResponse getTrustedEmployersResponse,
            GetReservationsResult getReservationsResult1,
            GetReservationsResult getReservationsResult2,
            GetReservationsResult getReservationsResult3,
            string hashedId,
            [Frozen] ReservationsWebConfiguration config,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getTrustedEmployersResponse);
            mockMediator
                .SetupSequence(mediator =>
                    mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getReservationsResult1)
                .ReturnsAsync(getReservationsResult2)
                .ReturnsAsync(getReservationsResult3);
            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);

            var expectedReservations = new List<ReservationViewModel>();
            expectedReservations.AddRange(getReservationsResult1.Reservations.Select(reservation =>
                new ReservationViewModel(reservation, config.ApprenticeUrl)));
            expectedReservations.AddRange(getReservationsResult2.Reservations.Select(reservation =>
                new ReservationViewModel(reservation, config.ApprenticeUrl)));
            expectedReservations.AddRange(getReservationsResult3.Reservations.Select(reservation =>
                new ReservationViewModel(reservation, config.ApprenticeUrl)));

            var result = await controller.Manage(routeModel) as ViewResult;

            result.Should().NotBeNull();
            result.ViewName.Should().Be(ViewNames.ProviderManage);
            var viewModel = result.Model as ManageViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Reservations.Should().BeEquivalentTo(expectedReservations,
                options => options.ExcludingMissingMembers().ExcludingFields().Excluding(c=>c.ApprenticeUrl));
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Gets_List_Of_Reservations_For_Single_Employer_Account(
            ReservationsRouteModel routeModel,
            GetReservationsResult getReservationsResult,
            long decodedAccountId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getReservationsResult);
            mockEncodingService
                .Setup(service => service.Decode(routeModel.EmployerAccountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            await controller.Manage(routeModel);

            mockMediator.Verify(mediator =>
                    mediator.Send(
                        It.IsAny<GetTrustedEmployersQuery>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
            mockMediator.Verify(mediator =>
                    mediator.Send(
                        It.Is<GetReservationsQuery>(query => query.AccountId == decodedAccountId),
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Returns_List_Of_Reservations_For_Single_Employer_Account(
            ReservationsRouteModel routeModel,
            GetReservationsResult getReservationsResult,
            string hashedId,
            [Frozen] ReservationsWebConfiguration config,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getReservationsResult);
            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);

            var expectedReservations = new List<ReservationViewModel>();
            expectedReservations.AddRange(
                getReservationsResult.Reservations.Select(
                    reservation => new ReservationViewModel(reservation, config.ApprenticeUrl)));

            var result = await controller.Manage(routeModel) as ViewResult;

            result.Should().NotBeNull();
            result.ViewName.Should().Be(ViewNames.EmployerManage);
            var viewModel = result.Model as ManageViewModel;
            viewModel.Should().NotBeNull();
            viewModel.Reservations.Should().BeEquivalentTo(expectedReservations,
                options => options.ExcludingMissingMembers());
        }

        [Test, MoqAutoData]
        public async Task UkPrn_Will_Be_Populated_From_RouteModel_For_Each_Reservation(
             ReservationsRouteModel routeModel,
            GetTrustedEmployersResponse getTrustedEmployersResponse,
            Reservation reservation,
             Reservation reservationTwo,
            string hashedId,
            [Frozen] ReservationsWebConfiguration config,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getTrustedEmployersResponse);

            reservation.ProviderId = null;
            routeModel.UkPrn = 2000032;

            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetReservationsResult
                {
                    Reservations = new []{reservation, reservationTwo }
                });

            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);
            
            var result = await controller.Manage(routeModel) as ViewResult;
            
            var viewModel = result?.Model as ManageViewModel;
            viewModel.Should().NotBeNull();

            Assert.IsTrue(viewModel.Reservations.All(c=>c.ApprenticeUrl.StartsWith($"{config.ApprenticeUrl}/{routeModel.UkPrn}/")));

        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Provider_Has_Not_TrustedEmployers_A_NoPermissions_View_Is_Returned(
            ReservationsRouteModel routeModel,
            GetTrustedEmployersResponse getTrustedEmployersResponse,
            Reservation reservation,
            string hashedId,
            [Frozen] ReservationsWebConfiguration config,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse{Employers = new List<Employer>()});

            var result = await controller.Manage(routeModel) as ViewResult;

            result.ViewName.Should().Be("NoPermissions");
        }

    }
}