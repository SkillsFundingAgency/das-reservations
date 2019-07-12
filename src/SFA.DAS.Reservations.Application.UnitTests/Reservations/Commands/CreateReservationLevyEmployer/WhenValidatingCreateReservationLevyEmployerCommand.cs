using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CreateReservationLevyEmployer
{
    [TestFixture]
    public class WhenValidatingCreateReservationLevyEmployerCommand
    {
        private CreateReservationLevyEmployerCommandValidator _validator;
        
        [SetUp]
        public void Arrange()
        {
            _validator = new CreateReservationLevyEmployerCommandValidator();
        }

        [Test]
        public async Task AndTheCommandIsInvalid_ThenAddsError()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = -234,
                AccountLegalEntityId = -3
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));

        }
        [Test]
        public async Task AndTheCommandIsValid_ThenNoErrorsAdded()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = 334,
                AccountLegalEntityId = 324234
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.True(result.IsValid());

        }
        [Test]
        public async Task AndOnlyAccountIdInvalid_ThenAddsError()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = -234,
                AccountLegalEntityId = 5131
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.False(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));

        }
        [Test]
        public async Task AndOnlyAccountLegalEntityIdInvalid_ThenAddsError()
        {
            //Arrange
            var command = new CreateReservationLevyEmployerCommand
            {
                AccountId = 34,
                AccountLegalEntityId = -234
            };

            //Act
            var result = await _validator.ValidateAsync(command);

            //Assert
            Assert.False(result.IsValid());
            Assert.False(result.ValidationDictionary.ContainsKey(nameof(command.AccountId)));
            Assert.True(result.ValidationDictionary.ContainsKey(nameof(command.AccountLegalEntityId)));
        }
    }
}
