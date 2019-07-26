using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Providers.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Employers;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Providers.Queries.GetLegalEntityAccount
{
    public class WhenGettingAccountLegalEntity
    {
        private GetAccountLegalEntityQuery _query;
        private Mock<IValidator<GetAccountLegalEntityQuery>> _validator;
        private GetAccountLegalEntityQueryHandler _handler;
        private Mock<IProviderService> _providerService;
        private Mock<IEncodingService> _encodingService;
        private AccountLegalEntity _expectedLegalEntity;
        private const long EmployerExpectedLegalEntityId = 10;

        [SetUp]
        public void Arrange()
        {
            _expectedLegalEntity = new AccountLegalEntity
            {
                AccountId = "1",
                AccountLegalEntityId = 2,
                AccountLegalEntityName = "Test entity",
                AccountLegalEntityPublicHashedId = "ABC123",
                LegalEntityId = 3,
                ReservationLimit = 10
            };

            _query = new GetAccountLegalEntityQuery();
            _validator = new Mock<IValidator<GetAccountLegalEntityQuery>>();
            _providerService = new Mock<IProviderService>();
            _encodingService = new Mock<IEncodingService>();

            _handler = new GetAccountLegalEntityQueryHandler(_providerService.Object, _encodingService.Object, _validator.Object);

            _providerService.Setup(s => s.GetAccountLegalEntityById(It.IsAny<long>()))
                .ReturnsAsync(_expectedLegalEntity);

            _encodingService.Setup(s => s.Decode(It.IsAny<string>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(EmployerExpectedLegalEntityId);

            _validator.Setup(v => v.ValidateAsync(It.IsAny<GetAccountLegalEntityQuery>()))
                .ReturnsAsync(() => new ValidationResult());
        }
        

        [Test]
        public async Task ThenWillDecodePublicHashedId()
        {
            //Act
            await _handler.Handle(_query, CancellationToken.None);

            //Assert
            _encodingService.Verify(s => s.Decode(_query.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId), Times.Once);
        }

        [Test]
        public async Task ThenWillCallServiceToGetAccount()
        {
            //Act
            await _handler.Handle(_query, CancellationToken.None);

            //Assert
            _providerService.Verify(s => s.GetAccountLegalEntityById(EmployerExpectedLegalEntityId), Times.Once);
        }

        [Test]
        public async Task ThenWillReturnAccountLegalEntity()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            Assert.AreEqual(_expectedLegalEntity, result.LegalEntity);
        }

        [Test]
        public void ThenWillThrowExceptionIfValidationFails()
        {
            //Arrange
            _validator.Setup(v => v.ValidateAsync(_query)).ReturnsAsync(() => new ValidationResult
                {ValidationDictionary = new Dictionary<string, string> {{"Test Error", "Error message"}}});

            //Act
            Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(_query, CancellationToken.None));
        }

        [Test]
        public void ThenWillThrowExceptionIfEncodingServiceThrowsException()
        {
            //Arrange
            var expectedException = new Exception();

            _encodingService.Setup(s => s.Decode(It.IsAny<string>(), EncodingType.PublicAccountLegalEntityId))
                .Throws(expectedException);

            //Act
            var exception = Assert.ThrowsAsync<Exception>(() => _handler.Handle(_query, CancellationToken.None));

            //Assert
            Assert.AreEqual(expectedException, exception);
        }

        [Test]
        public void ThenWillThrowExceptionIfProviderServiceThrowsException()
        {
            //Arrange
            var expectedException = new Exception();

            _providerService.Setup(s => s.GetAccountLegalEntityById(It.IsAny<long>()))
                .ThrowsAsync(expectedException);

            //Act
            var exception = Assert.ThrowsAsync<Exception>(() => _handler.Handle(_query, CancellationToken.None));

            //Assert
            Assert.AreEqual(expectedException, exception);
        }


    }
}
