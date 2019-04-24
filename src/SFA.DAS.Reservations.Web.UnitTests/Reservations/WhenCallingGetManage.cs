using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetManage
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Gets_List_Of_Reservations_For_All_Trusted_Employer_Accounts(
            ReservationsRouteModel routeModel,
            GetTrustedEmployersResponse getTrustedEmployersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getTrustedEmployersResponse);

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
                            It.Is<GetReservationsQuery>(query => query.AccountId == employer.AccountId.ToString()),
                            It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Gets_List_Of_Reservations_For_Single_Employer_Account(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;

            await controller.Manage(routeModel);

            mockMediator.Verify(mediator =>
                    mediator.Send(
                        It.IsAny<GetTrustedEmployersQuery>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
            mockMediator.Verify(mediator =>
                    mediator.Send(
                        It.Is<GetReservationsQuery>(query => query.AccountId == routeModel.EmployerAccountId),
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}