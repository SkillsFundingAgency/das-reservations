using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

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

            
            mockMediator.Setup(m => m.Send(It.IsAny<CacheCreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CacheReservationResult{ Id = Guid.NewGuid() });

            mockHashingService.Setup(s => s.DecodeValue(hashedAccountId))
                .Returns(expectedEmployerAccountId);   
            
            //Act
            await controller.Index(hashedAccountId);
            
            //Assert
            mockMediator.Verify(m => m.Send(It.Is<CacheCreateReservationCommand>( c =>
                   c.AccountId.Equals(expectedEmployerAccountId) &&
                   c.AccountLegalEntityId != default(long) &&
                   !string.IsNullOrEmpty(c.AccountLegalEntityName)), It.IsAny<CancellationToken>()));
                
        }

        [Test, MoqAutoData]
        public async Task Then_Reservation_Id_Will_Be_Returned(
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //Assign
            var expectedId = Guid.NewGuid();
            
            mockMediator.Setup(m => m.Send(It.IsAny<CacheCreateReservationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CacheReservationResult{ Id = expectedId });
            
            //Act
            var result = await controller.Index("ABC123") as ViewResult;
            var viewModel = result?.Model as ReservationViewModel;

            //Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(expectedId, viewModel.Id);
        }
    }
}
