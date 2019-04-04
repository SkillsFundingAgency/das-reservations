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
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CacheReservationEmployer
{
    public class WhenCachingReservationEmployer
    {
        private Mock<IValidator<CacheReservationEmployerCommand>> _mockValidator;
        private Mock<ICacheStorageService> _mockCacheStorageService;
        private CacheReservationEmployerCommandHandler _commandHandler;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _mockValidator = fixture.Freeze<Mock<IValidator<CacheReservationEmployerCommand>>>();
           
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CacheReservationEmployerCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockCacheStorageService = fixture.Freeze<Mock<ICacheStorageService>>();

            _commandHandler = fixture.Create<CacheReservationEmployerCommandHandler>();
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Command(CacheReservationEmployerCommand command)
        {
            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Throws_ArgumentException(
            CacheReservationEmployerCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            //Assign
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);

            //Act
            Func<Task> act = async () => { await _commandHandler.Handle(command, CancellationToken.None); };

            //Assert
            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c=>c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }
        
        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Does_Not_Cache_Reservation(
            CacheReservationEmployerCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            //Assign
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);
            
            //Act
             Assert.ThrowsAsync<ValidationException>(() => _commandHandler.Handle(command, CancellationToken.None));

            //Assert
            _mockCacheStorageService.Verify(s => s.SaveToCache(It.IsAny<string>(), It.IsAny<CachedReservation>(), It.IsAny<int>()), Times.Never);
        }

        [Test, AutoData]
        public async Task Then_Calls_Cache_Service_To_Save_Reservation(CacheReservationEmployerCommand command)
        {
            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockCacheStorageService.Verify(service => service.SaveToCache(
                It.IsAny<string>(), 
                It.Is<CachedReservation>(c => c.Id.Equals(command.Id) &&
                    c.AccountId.Equals(command.AccountId) &&
                    c.AccountLegalEntityId.Equals(command.AccountLegalEntityId) &&
                    c.AccountLegalEntityName.Equals(command.AccountLegalEntityName) &&
                    c.AccountLegalEntityPublicHashedId.Equals(command.AccountLegalEntityPublicHashedId)), 
                1));
        }
    }
}
