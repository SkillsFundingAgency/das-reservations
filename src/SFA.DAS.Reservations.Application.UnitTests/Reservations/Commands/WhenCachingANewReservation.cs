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
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.UnitTests.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenCachingANewReservation
    {
        private Mock<IValidator<BaseCreateReservationCommand>> _mockValidator;
        private Mock<ICacheStorageService> _mockCacheStorageService;
        private CacheReservationCommandHandler _commandHandler;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _mockValidator = fixture.Freeze<Mock<IValidator<BaseCreateReservationCommand>>>();
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CacheReservationCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockCacheStorageService = fixture.Freeze<Mock<ICacheStorageService>>();

            _commandHandler = fixture.Create<CacheReservationCommandHandler>();
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Command(
            CacheReservationCommand command)
        {
            command.StartDate = "2019-01";

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Throws_ArgumentException(
            CacheReservationCommand command,
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
        public async Task Then_Calls_Cache_Service_To_Save_Reservation(
            CacheReservationCommand command)
        {
            command.Id = null;
            var originalCommand = command.Clone();

            var result = await _commandHandler.Handle(command, CancellationToken.None);

            _mockCacheStorageService.Verify(service => service.SaveToCache(It.IsAny<string>(), command, 1));
            result.Id.Should().NotBe(originalCommand.Id.GetValueOrDefault());
            result.Id.Should().NotBeEmpty();
        }

        [Test, AutoData]
        public async Task And_Is_Existing_Cache_Item_Then_Calls_Cache_Service_Using_Same_Key(
            CacheReservationCommand command)
        {
            var originalCommand = command.Clone();
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            _mockCacheStorageService.Verify(service => service.SaveToCache(originalCommand.Id.ToString(), command, 1));
            result.Id.Should().Be(originalCommand.Id.Value);
        }
    }
}