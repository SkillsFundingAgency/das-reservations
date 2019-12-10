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
using SFA.DAS.Reservations.Web.Controllers;
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

            var result = await controller.TransactorSignAgreement(routeModel) as ViewResult;

            result.ViewName.Should().Be("TransactorSignAgreement");
            var model = result.Model as SignAgreementViewModel;
            model.BackRouteName.Should().Be(routeModel.PreviousPage);
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
                .Where(user => user.Role.Equals("Owner", StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(user => user.Name)
                .Select(user => (EmployerAccountUserViewModel)user));
        }

        // todo: exception
    }
}