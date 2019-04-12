using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    [TestFixture]
    public class WhenCallingPostSelectLegalEntity
    {
        [Test, MoqAutoData]
        public async Task Then_Get_Legal_Entity_Details(
            ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            await controller.PostSelectLegalEntity(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetLegalEntitiesQuery>(query => query.AccountId == routeModel.EmployerAccountId), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}