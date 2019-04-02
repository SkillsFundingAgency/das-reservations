using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenValidatingACacheReservationEmployerCommand
    {
        [Test, AutoData]
        public async Task And_No_Id_Then_Invalid(
            CacheReservationEmployerCommand command,
            CacheReservationEmployerCommandValidator validator)
        {
            command.Id = Guid.Empty;

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.Id))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.Id)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountId_Less_Than_One_Then_Invalid(
            CacheReservationEmployerCommand command,
            CacheReservationEmployerCommandValidator validator)
        {
            command.AccountId = default(long);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityId_Less_Than_One_Then_Invalid(
            CacheReservationEmployerCommand command,
            CacheReservationEmployerCommandValidator validator)
        {
            command.AccountLegalEntityId = default(long);

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityName_Null_Then_Invalid(
            CacheReservationEmployerCommand command,
            CacheReservationEmployerCommandValidator validator)
        {
            command.AccountLegalEntityName = null;

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityName))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityName)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityName_Whitespace_Then_Invalid(
            CacheReservationEmployerCommand command,
            CacheReservationEmployerCommandValidator validator)
        {
            command.AccountLegalEntityName = " ";

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityName))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityName)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityPublicHashedId_Null_Then_Invalid(
            CacheReservationEmployerCommand command,
            CacheReservationEmployerCommandValidator validator)
        {
            command.AccountLegalEntityPublicHashedId = null;

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_AccountLegalEntityPublicHashedId_Whitespace_Then_Invalid(
            CacheReservationEmployerCommand command,
            CacheReservationEmployerCommandValidator validator)
        {
            command.AccountLegalEntityPublicHashedId = " ";

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId))
                .WhichValue.Should().Be($"{nameof(CacheReservationEmployerCommand.AccountLegalEntityPublicHashedId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Properties_Ok_Then_Valid(
            CacheReservationEmployerCommand command,
            CacheReservationEmployerCommandValidator validator)
        {
            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
        }
    }
}