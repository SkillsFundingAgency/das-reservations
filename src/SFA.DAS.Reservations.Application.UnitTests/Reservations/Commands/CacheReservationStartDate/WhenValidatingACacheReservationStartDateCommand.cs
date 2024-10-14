using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CacheReservationStartDate
{
    [TestFixture]
    public class WhenValidatingACacheReservationStartDateCommand
    {
        [Test]
        public async Task And_No_Id_Then_Invalid()
        {
            var validator = new CacheReservationStartDateCommandValidator();
            var command = new CacheReservationStartDateCommand
            {
                TrainingDate = new TrainingDateModel { StartDate = DateTime.Now }
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);

            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationStartDateCommand.Id))
                .WhoseValue.Should().Be($"{nameof(CacheReservationStartDateCommand.Id)} has not been supplied");
        }

        [Test]
        public async Task And_No_Training_Date_Then_Invalid()
        {
            var validator = new CacheReservationStartDateCommandValidator();
            var command = new CacheReservationStartDateCommand
            {
                Id = Guid.NewGuid()
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);

            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationStartDateCommand.TrainingDate))
                .WhoseValue.Should().Be($"{nameof(CacheReservationStartDateCommand.TrainingDate)} has not been supplied");
        }

        [Test]
        public async Task And_No_Training_Date_Start_Date_Then_Invalid()
        {
            var validator = new CacheReservationStartDateCommandValidator();
            var command = new CacheReservationStartDateCommand
            {
                Id = Guid.NewGuid(),
                TrainingDate = new TrainingDateModel()
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);

            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationStartDateCommand.TrainingDate))
                .WhoseValue.Should().Be($"{nameof(CacheReservationStartDateCommand.TrainingDate.StartDate)} must be set on {nameof(CacheReservationStartDateCommand.TrainingDate)}");
        }

        [Test]
        public async Task And_All_Fields_Valid_Then_Valid()
        {
            var validator = new CacheReservationStartDateCommandValidator();
            var command = new CacheReservationStartDateCommand
            {
                Id = Guid.NewGuid(),
                TrainingDate = new TrainingDateModel {StartDate = DateTime.Now}
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
        }
    }
}