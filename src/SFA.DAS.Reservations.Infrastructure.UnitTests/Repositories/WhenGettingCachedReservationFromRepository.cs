using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using SFA.DAS.Reservations.Infrastructure.Repositories;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Repositories
{
    public class WhenGettingCachedReservationFromRepository
    {
        private Mock<ICacheStorageService> _cacheService;
        private Mock<IReservationAuthorisationService> _authorisationService;
        private CachedReservationRepository _repository;
        private CachedReservation _reservation;

        [SetUp]
        public void Arrange()
        {
            _cacheService = new Mock<ICacheStorageService>();
            _authorisationService = new Mock<IReservationAuthorisationService>();
            _reservation = new CachedReservation();
            _repository = new CachedReservationRepository(_cacheService.Object, _authorisationService.Object);
        }

        [Test]
        public async Task Then_Will_Check_If_Provider_Has_Reservation_Access()
        {
            //Arrange
            _cacheService.Setup(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()))
                .ReturnsAsync(_reservation);

            _authorisationService.Setup(s =>
                    s.ProviderReservationAccessAllowed(It.IsAny<uint>(), It.IsAny<CachedReservation>()))
                .Returns(true);

            //Act
            await _repository.GetProviderReservation(Guid.NewGuid(), 1);

            //Assert
            _authorisationService.Verify(s => s.ProviderReservationAccessAllowed(1, _reservation), Times.Once);
        }

        [Test]
        public async Task Then_Will_Return_Reservation_If_Access_Granted()
        {
            //Arrange
            _cacheService.Setup(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()))
                .ReturnsAsync(_reservation);

            _authorisationService.Setup(s =>
                    s.ProviderReservationAccessAllowed(It.IsAny<uint>(), It.IsAny<CachedReservation>()))
                .Returns(true);

            //Act
            var result = await _repository.GetProviderReservation(Guid.NewGuid(), 1);

            //Assert
            Assert.AreEqual(_reservation, result);
        }

        [Test]
        public void Then_Will_Throw_Exception_If_Reservation_Access_Denied()
        {
            //Arrange
            _cacheService.Setup(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()))
                .ReturnsAsync(_reservation);

            _authorisationService.Setup(s =>
                    s.ProviderReservationAccessAllowed(It.IsAny<uint>(), It.IsAny<CachedReservation>()))
                .Returns(false);

            //Act + assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _repository.GetProviderReservation(Guid.NewGuid(), 1));
        }

        [Test]
        public void Then_Will_Throw_Exception_If_Reservation_Not_Found()
        {
            //Arrange
            _cacheService.Setup(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()))
                .ReturnsAsync((CachedReservation) null);

            _authorisationService.Setup(s =>
                    s.ProviderReservationAccessAllowed(It.IsAny<uint>(), It.IsAny<CachedReservation>()))
                .Returns(false);

            //Act + assert
            Assert.ThrowsAsync<CachedReservationNotFoundException>(() => _repository.GetProviderReservation(Guid.NewGuid(), 1));
        }

        [Test]
        public void Then_Will_Throw_Exception_If_ReservationId_Invalid()
        {
            //Act + assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _repository.GetProviderReservation(Guid.Empty, 1));
            Assert.AreEqual("id", exception.ParamName);

            _cacheService.Verify(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()), Times.Never);
            _authorisationService.Verify(s => s.ProviderReservationAccessAllowed(It.IsAny<uint>(), It.IsAny<CachedReservation>()), Times.Never);
        }

        [Test]
        public void Then_Will_Throw_Exception_If_UkPrn_Invalid()
        {
            //Act + assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _repository.GetProviderReservation(Guid.NewGuid(), default(uint)));
            Assert.AreEqual("ukPrn", exception.ParamName);

            _cacheService.Verify(s => s.RetrieveFromCache<CachedReservation>(It.IsAny<string>()), Times.Never);
            _authorisationService.Verify(s => s.ProviderReservationAccessAllowed(It.IsAny<uint>(), It.IsAny<CachedReservation>()), Times.Never);
        }
    }
}
