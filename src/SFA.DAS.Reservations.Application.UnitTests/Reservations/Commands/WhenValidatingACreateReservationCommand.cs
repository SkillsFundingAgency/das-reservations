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
            CreateReservationCommandValidator validator)
        {
            var command = new CacheCreateReservationCommand
            {
                Id = Guid.NewGuid(),
                AccountId = 0,
                StartDate = "2018-09",
                AccountLegalEntityName = "Test Name"
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(ICreateReservationCommand.AccountId))
                .WhichValue.Should().Be($"{nameof(ICreateReservationCommand.AccountId)} has not been supplied");
        }

        [TestCase("19-a")]
        [TestCase("19-")]
        [TestCase("a-1")]
        [TestCase("1-1")]
        [TestCase("a-a")]
        [TestCase("a")]
        [TestCase("-")]
        public async Task And_StartDate_Is_Not_In_The_Correct_Format_Then_Invalid(string startDate)
        {
            var validator = new CreateReservationCommandValidator();
            var command = new CacheCreateReservationCommand
            {
                Id = Guid.NewGuid(),
                AccountId = 1,
                StartDate = startDate,
                AccountLegalEntityName = "Test Name"
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
            CreateReservationCommandValidator validator)
        {
            var command = new CacheCreateReservationCommand
            {
                AccountId = 0,
                StartDate = "",
                AccountLegalEntityName = ""
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(3);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CreateReservationCommand.AccountId))
                .And.ContainKey(nameof(CreateReservationCommand.StartDate))
                .And.ContainKey(nameof(CreateReservationCommand.AccountLegalEntityName));
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            CreateReservationCommandValidator validator)
        {
            var command = new CacheCreateReservationCommand
            {
                AccountId = 1,
                StartDate = "2019-07",
                AccountLegalEntityName = "Test Name"
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
