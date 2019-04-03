using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenValidatingACacheReservationStartDateCommand
    {
        [TestCase("19-a")]
        [TestCase("19-")]
        [TestCase("a-1")]
        [TestCase("1-1")]
        [TestCase("a-a")]
        [TestCase("a")]
        [TestCase("-")]
        public async Task And_StartDate_Is_Not_In_The_Correct_Format_Then_Invalid(string startDate)
        {
            var validator = new CacheReservationStartDateCommandValidator();
            var command = new CacheReservationStartDateCommand
            {
                Id = Guid.NewGuid(),
                StartDate = startDate
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationStartDateCommand.StartDate))
                .WhichValue.Should().Be($"{nameof(CacheReservationStartDateCommand.StartDate)} has not been supplied");
        }

        [Test]
        public async Task And_No_Id_Then_Invalid()
        {
            var validator = new CacheReservationStartDateCommandValidator();
            var command = new CacheReservationStartDateCommand
            {
                StartDate = "2020-12"
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationStartDateCommand.Id))
                .WhichValue.Should().Be($"{nameof(CacheReservationStartDateCommand.Id)} has not been supplied");
        }

        [Test]
        public async Task And_All_Fields_Valid_Then_Valid()
        {
            var validator = new CacheReservationStartDateCommandValidator();
            var command = new CacheReservationStartDateCommand
            {
                Id = Guid.NewGuid(),
                StartDate = "2020-02"
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
        }
    }
}