using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetSelectReservation
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Gets_Employer_From_Trusted_Employers(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            await controller.SelectReservation(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetTrustedEmployersQuery>(query => query.UkPrn == routeModel.UkPrn), 
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Employer_Not_Found_Then_Redirect_To_Error_Page(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderError);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Gets_Reservations_For_Employer_Account(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            await controller.SelectReservation(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetAvailableReservationsQuery>(query => query.AccountId == matchedEmployer.AccountId), 
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Has_Reservations_Then_Adds_Reservations_To_Model(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.ProviderSelect);
            var actualModel = result.Model as SelectReservationViewModel;
            actualModel.Should().NotBeNull();
            actualModel.AvailableReservations.Should().BeEquivalentTo(
                reservationsResult.Reservations
                    .Select(reservation => new AvailableReservationViewModel(reservation)));
        }
    }
}