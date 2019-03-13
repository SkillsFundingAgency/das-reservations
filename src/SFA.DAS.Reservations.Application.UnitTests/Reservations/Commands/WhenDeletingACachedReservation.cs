using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenDeletingACachedReservation
    {
        [Test, MoqAutoData]
        public async Task Then_Cache_Service_Removes_From_Cache(
            DeleteCachedReservationCommand command,
            [Frozen] Mock<ICacheStorageService> mockCacheService,
            DeleteCachedReservationCommandHandler commandHandler)
        {
            await commandHandler.Handle(command, CancellationToken.None);

            mockCacheService.Verify(service => service.DeleteFromCache(command.Id.ToString()), Times.Once);
        }
    }
}