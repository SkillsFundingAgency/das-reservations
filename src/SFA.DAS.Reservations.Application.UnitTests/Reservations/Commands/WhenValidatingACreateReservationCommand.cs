using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenValidatingACreateReservationCommand
    {
        [Test, AutoData]
        public async Task And_AccountId_Less_Than_One_Then_Invalid(
            CreateReservationValidator validator)
        {
            var command = new CreateReservationCommand
            {
                AccountId = "0",
                StartDate = DateTime.Today
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CreateReservationCommand.AccountId))
                .WhichValue.Should().Be($"{nameof(CreateReservationCommand.AccountId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_StartDate_Is_MinValue_Then_Invalid(
            CreateReservationValidator validator)
        {
            var command = new CreateReservationCommand
            {
                AccountId = "1",
                StartDate = DateTime.MinValue
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CreateReservationCommand.StartDate))
                .WhichValue.Should().Be($"{nameof(CreateReservationCommand.StartDate)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Fields_Invalid_Then_Returns_All_Errors(
            CreateReservationValidator validator)
        {
            var command = new CreateReservationCommand
            {
                AccountId = "0",
                StartDate = DateTime.MinValue
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(2);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CreateReservationCommand.AccountId))
                .And.ContainKey(nameof(CreateReservationCommand.StartDate));
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            CreateReservationValidator validator)
        {
            var command = new CreateReservationCommand
            {
                AccountId = "1",
                StartDate = DateTime.Today
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
