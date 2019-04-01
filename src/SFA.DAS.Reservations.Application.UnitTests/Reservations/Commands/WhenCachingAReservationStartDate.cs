﻿using System;
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
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenCachingAReservationStartDate
    {
        private Mock<IValidator<CacheReservationStartDateCommand>> _mockValidator;
        private Mock<ICacheStorageService> _mockCacheStorageService;
        private CacheReservationStartDateCommandHandler _commandHandler;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _mockValidator = fixture.Freeze<Mock<IValidator<CacheReservationStartDateCommand>>>();
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CacheReservationStartDateCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockCacheStorageService = fixture.Freeze<Mock<ICacheStorageService>>();

            _commandHandler = fixture.Create<CacheReservationStartDateCommandHandler>();
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
    }
}