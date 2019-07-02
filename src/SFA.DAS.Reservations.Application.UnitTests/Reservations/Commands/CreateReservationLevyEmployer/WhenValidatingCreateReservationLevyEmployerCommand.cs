using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CreateReservationLevyEmployer
{
    [TestFixture]
    public class WhenValidatingCreateReservationLevyEmployerCommand
    {
        [Test]
        public async Task AndTheCommandIsInvalid_ThenAddsError()
        {
            //Arrange
            var accountId = -234;
            var accountLegalEntityId = -3;
            var command = new CreateReservationLevyEmployerCommand();
            var validator = new CreateReservationLevyEmployerCommandValidator();
            command.AccountId = accountId;
            command.AccountLegalEntityId = accountLegalEntityId;

            //Act
            var result = await validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));

        }
        [Test]
        public async Task AndTheCommandIsValid_ThenNoErrorsAdded()
        {
            //Arrange
            var accountId = 334;
            var accountLegalEntityId = 324234;
            var command = new CreateReservationLevyEmployerCommand();
            var validator = new CreateReservationLevyEmployerCommandValidator();
            command.AccountId = accountId;
            command.AccountLegalEntityId = accountLegalEntityId;

            //Act
            var result = await validator.ValidateAsync(command);

            //Assert
            Assert.True(result.IsValid());

        }
        [Test]
        public async Task AndOnlyAccountIdInvalid_ThenAddsError()
        {
            //Arrange
            var accountId = -234;
            var accountLegalEntityId = 5131;
            var command = new CreateReservationLevyEmployerCommand();
            var validator = new CreateReservationLevyEmployerCommandValidator();
            command.AccountId = accountId;
            command.AccountLegalEntityId = accountLegalEntityId;

            //Act
            var result = await validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.False(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));

        }
        [Test]
        public async Task AndOnlyAccountLegalEntityIdInvalid_ThenAddsError()
        {
            //Arrange
            var accountId = 34;
            var accountLegalEntityId = -234;
            var command = new CreateReservationLevyEmployerCommand();
            var validator = new CreateReservationLevyEmployerCommandValidator();
            command.AccountId = accountId;
            command.AccountLegalEntityId = accountLegalEntityId;

            //Act
            var result = await validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.False(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));
        }
    }
}
