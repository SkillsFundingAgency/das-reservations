using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Web.Controllers;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheStartPage
    {
        [Test, MoqAutoData]
        public async Task Then_Reservation_Cache_WIll_Be_Created(
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IHashingService> mockHashingService,
            EmployerReservationsController controller)
        {
            //Assign
            var hashedAccountId = "ABC123";
            var expectedEmployerAccountId = 123;
            
            mockMediator.Setup(m => m.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            mockHashingService.Setup(s => s.DecodeValue(hashedAccountId))
                .Returns(expectedEmployerAccountId);   
            
            //Act
            await controller.Index(hashedAccountId);
            
            //Assert
            mockMediator.Verify(m => m.Send(It.Is<CacheReservationEmployerCommand>( c =>
                   c.AccountId.Equals(expectedEmployerAccountId) &&
                   c.AccountLegalEntityId != default(long) &&
                   !string.IsNullOrEmpty(c.AccountLegalEntityName)), It.IsAny<CancellationToken>()));
        }
    }
}
