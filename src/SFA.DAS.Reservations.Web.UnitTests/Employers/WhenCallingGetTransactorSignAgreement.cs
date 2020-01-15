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
using SFA.DAS.Reservations.Application.Employers.Queries.GetAccountUsers;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenCallingGetTransactorSignAgreement
    {
        [Test, MoqAutoData]
        public async Task Then_Sets_ViewModel(
            ReservationsRouteModel routeModel,
            GetAccountUsersResponse usersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAccountUsersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(usersResponse);
            routeModel.IsFromSelect = null;

            var result = await controller.TransactorSignAgreement(routeModel) as ViewResult;

            result.ViewName.Should().Be("TransactorSignAgreement");
            var model = result.Model as SignAgreementViewModel;
            model.BackRouteName.Should().Be(routeModel.PreviousPage);
            model.IsUrl.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Then_Sets_The_IsUrl_Property_If_Come_From_Select_Reservation(
            ReservationsRouteModel routeModel,
            GetAccountUsersResponse usersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAccountUsersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(usersResponse);
            routeModel.IsFromSelect = true;

            var result = await controller.TransactorSignAgreement(routeModel) as ViewResult;
            
            result.ViewName.Should().Be("TransactorSignAgreement");
            var model = result.Model as SignAgreementViewModel;
            model.BackRouteName.Should().Be(routeModel.PreviousPage);
            model.IsUrl.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task Then_Sets_Owner_Users_On_ViewModel(
            ReservationsRouteModel routeModel,
            GetAccountUsersResponse usersResponse,
            long decodedAccountId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            EmployerReservationsController controller)
        {
            usersResponse.AccountUsers.First().Role = "Owner";
            mockEncodingService
                .Setup(service => service.Decode(
                    routeModel.EmployerAccountId, 
                    EncodingType.AccountId))
                .Returns(decodedAccountId);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAccountUsersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(usersResponse);

            var result = await controller.TransactorSignAgreement(routeModel) as ViewResult;

            var model = result.Model as SignAgreementViewModel;
            model.OwnersOfThisAccount.Should().BeEquivalentTo(usersResponse.AccountUsers
                .Where(user => user.Role.Equals(EmployerUserRole.Owner.ToString(), StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(user => user.Name)
                .Select(user => (EmployerAccountUserViewModel)user));
        }

        [Test, MoqAutoData]
        public async Task And_Exception_Then_Redirect_To_Error_500(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAccountUsersQuery>(),
                    It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            var result = await controller.TransactorSignAgreement(routeModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }
    }
}