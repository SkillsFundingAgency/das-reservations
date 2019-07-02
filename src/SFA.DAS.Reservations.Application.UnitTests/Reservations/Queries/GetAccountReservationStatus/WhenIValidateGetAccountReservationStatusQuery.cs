using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetAccountReservationStatus
{
    [TestFixture]
    public class WhenIValidateGetAccountReservationStatusQuery
    {
        private IValidator<GetAccountReservationStatusQuery> _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new GetAccountReservationStatusQueryValidator();
        }

        [Test]
        public async Task AndTheQueryIsValid_ThenValidationPasses()
        {
            //Arrange
            var query = new GetAccountReservationStatusQuery(){AccountId = 12342};

            //Act
            var result = await _validator.ValidateAsync(query);

            //Assert
            Assert.True(result.IsValid());
        }

        [Test]
        public async Task AndTheAccountIdIsDefault_ThenValidationFails()
        {
            //Arrange
            var query = new GetAccountReservationStatusQuery() { AccountId = default(long) };

            //Act
            var result = await _validator.ValidateAsync(query);

            //Assert
            Assert.False(result.IsValid());
        }
    }
}
