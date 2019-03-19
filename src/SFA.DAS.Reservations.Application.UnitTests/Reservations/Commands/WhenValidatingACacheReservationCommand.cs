using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenValidatingACacheReservationCommand
    {
        [Test, AutoData]
        public async Task And_AccountId_Less_Than_One_Then_Invalid(
            CacheCreateReservationCommandValidator validator)
        {
            var command = new CacheCreateReservationCommand
            {
                Id = Guid.NewGuid(),
                AccountPublicHashedId = "0",
                StartDate = "2018-09"
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheCreateReservationCommand.AccountPublicHashedId))
                .WhichValue.Should().Be($"{nameof(CacheCreateReservationCommand.AccountPublicHashedId)} has not been supplied");
        }

        [Test]
        public async Task And_No_CommandId_And_No_StartDate_Then_Valid()
        {
            var validator = new CacheCreateReservationCommandValidator();

            var command = new CacheCreateReservationCommand
            {
                AccountPublicHashedId = "1"
            };

            var result = await validator.ValidateAsync(command);

            Assert.IsTrue(result.IsValid());
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
            var validator = new CacheCreateReservationCommandValidator();
            var command = new CacheCreateReservationCommand
            {
                Id = Guid.NewGuid(),
                AccountPublicHashedId = "1",
                StartDate = startDate
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheCreateReservationCommand.StartDate))
                .WhichValue.Should().Be($"{nameof(CacheCreateReservationCommand.StartDate)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Fields_Invalid_Then_Returns_All_Errors(
            CacheCreateReservationCommandValidator validator)
        {
            var command = new CacheCreateReservationCommand
            {
                Id = Guid.NewGuid(),
                AccountPublicHashedId = "0",
                StartDate = ""
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(2);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheCreateReservationCommand.AccountPublicHashedId))
                .And.ContainKey(nameof(CacheCreateReservationCommand.StartDate));
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            CacheCreateReservationCommandValidator validator)
        {
            var command = new CacheCreateReservationCommand
            {
                AccountPublicHashedId = "1",
                StartDate = "2019-07"
            };

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
