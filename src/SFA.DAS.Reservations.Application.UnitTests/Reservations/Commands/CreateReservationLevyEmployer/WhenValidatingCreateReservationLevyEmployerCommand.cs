using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CreateReservationLevyEmployer
{
    [TestFixture]
    public class WhenValidatingCreateReservationLevyEmployerCommand
    {
        [TestCase(Int64.MinValue, Int64.MinValue)]
        [TestCase(-454, -54545)]
        [TestCase(0,0)]
        public async Task AndTheCommandIsInvalid_ThenAddsError(long accountId, long accountLegalEntityId)
        {
            //Arrange
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

        [TestCase(Int64.MaxValue, Int64.MaxValue)]
        [TestCase(45484, 54545)]
        [TestCase(1, 1)]
        public async Task AndTheCommandIsValid_ThenNoErrorsAdded(long accountId, long accountLegalEntityId)
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand();
            var validator = new CreateReservationLevyEmployerCommandValidator();
            command.AccountId = accountId;
            command.AccountLegalEntityId = accountLegalEntityId;

            //Act
            var result = await validator.ValidateAsync(command);

            //Assert
            Assert.True(result.IsValid());
            Assert.True(result.ValidationDictionary.IsNullOrEmpty());

        }
    }
}
