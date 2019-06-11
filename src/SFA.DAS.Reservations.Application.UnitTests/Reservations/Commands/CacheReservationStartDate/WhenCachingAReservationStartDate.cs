using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Application.UnitTests.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CacheReservationStartDate
{
    [TestFixture]
    public class WhenCachingAReservationStartDate
    {
        private Mock<IValidator<CacheReservationStartDateCommand>> _mockValidator;
        private Mock<ICacheStorageService> _mockCacheStorageService;
        private Mock<ICachedReservationRespository> _mockCacheRepository;
        private CacheReservationStartDateCommandHandler _commandHandler;
        private CachedReservation _cachedReservation;
        

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});
            _cachedReservation = fixture.Create<CachedReservation>();

            _mockValidator = fixture.Freeze<Mock<IValidator<CacheReservationStartDateCommand>>>();
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CacheReservationStartDateCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockCacheStorageService = fixture.Freeze<Mock<ICacheStorageService>>();
            _mockCacheRepository = fixture.Freeze<Mock<ICachedReservationRespository>>();
            _mockCacheRepository.Setup(r => r.GetProviderReservation(It.IsAny<Guid>(), It.IsAny<uint>()))
                .ReturnsAsync(_cachedReservation);

            _commandHandler = new CacheReservationStartDateCommandHandler(
                _mockValidator.Object,
                _mockCacheStorageService.Object,
                _mockCacheRepository.Object);
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Command(
            CacheReservationStartDateCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);

            _mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, AutoData]
        public void And_Invalid_Then_It_Throws_ValidationException(
            CacheReservationStartDateCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await _commandHandler.Handle(command, CancellationToken.None); };

            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c=>c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, AutoData]
        public async Task And_Is_Existing_Cache_Item_Then_Calls_Cache_Service_Using_Same_Key(
            CacheReservationStartDateCommand command)
        {
            _cachedReservation.Id = command.Id;
            var originalCommand = command.Clone();

            await _commandHandler.Handle(command, CancellationToken.None);
            
            _mockCacheStorageService.Verify(service => service.SaveToCache(originalCommand.Id.ToString(), It.Is<CachedReservation>(reservation => 
                reservation.Id == originalCommand.Id && 
                reservation.TrainingDate.Equals(originalCommand.TrainingDate) &&
                reservation.AccountId == _cachedReservation.AccountId &&
                reservation.AccountLegalEntityId == _cachedReservation.AccountLegalEntityId &&
                reservation.AccountLegalEntityName == _cachedReservation.AccountLegalEntityName &&
                reservation.AccountLegalEntityPublicHashedId == _cachedReservation.AccountLegalEntityPublicHashedId &&
                reservation.CourseId == _cachedReservation.CourseId &&
                reservation.CourseDescription == _cachedReservation.CourseDescription), 1));
        }

        [Test, AutoData]
        public async Task Then_Gets_Provider_Cached_Reservation(CacheReservationStartDateCommand command)
        {
            await _commandHandler.Handle(command, CancellationToken.None);
            
            _mockCacheRepository.Verify(service => service.GetProviderReservation(command.Id, command.UkPrn), Times.Once);
            _mockCacheRepository.Verify(service => service.GetEmployerReservation(It.IsAny<Guid>()), Times.Never);
        }

        [Test, AutoData]
        public async Task Then_Gets_Employer_Cached_Reservation(CacheReservationStartDateCommand command)
        {
            command.UkPrn = default(uint);

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockCacheRepository.Verify(service => service.GetEmployerReservation(command.Id), Times.Once);
            _mockCacheRepository.Verify(service => service.GetProviderReservation(It.IsAny<Guid>(), It.IsAny<uint>()), Times.Never);
        }

        [Test, AutoData]
        public void And_CachedReservation_Not_Found_Then_It_Throws_Exception(
            CacheReservationStartDateCommand command)
        {
            var expectedException = new CachedReservationNotFoundException(command.Id);

            _mockCacheRepository.Setup(r => r.GetProviderReservation(It.IsAny<Guid>(), It.IsAny<uint>()))
                .ThrowsAsync(expectedException);

            var exception = Assert.ThrowsAsync<CachedReservationNotFoundException>(() =>
                _commandHandler.Handle(command, CancellationToken.None));

            Assert.AreEqual(expectedException, exception);
        }
    }
}