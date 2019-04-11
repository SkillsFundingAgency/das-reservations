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
    public class WhenCallingSelectLegalEntity
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Legal_Entities(
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            await controller.SelectLegalEntity(routeModel);

            mockMediator.Verify(mediator => mediator.Send(It.Is<GetLegalEntitiesQuery>(query => query.AccountId == routeModel.EmployerAccountId), It.IsAny<CancellationToken>()), Times.Once);
        }
        // todo: get legal entity details and add them to the view model
    }
}