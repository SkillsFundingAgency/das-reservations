using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetAccountReservationStatus
{
    [TestFixture]
    public class WhenIGetAccountReservationStatus
    {
        private Mock<IApiClient> _apiClient;
        private GetAccountReservationStatusQueryHandler _handler;
        private Mock<IValidator<GetAccountReservationStatusQuery>> _validator;
        private Mock<IOptions<ReservationsApiConfiguration>> _configOptions;


        [SetUp]
        public void SetUp()
        {
            _apiClient = new Mock<IApiClient>();
            _configOptions = new Mock<IOptions<ReservationsApiConfiguration>>();
            _configOptions.Setup(x => x.Value.Url).Returns("test/test");
            _validator = new Mock<IValidator<GetAccountReservationStatusQuery>>();
            _handler = new GetAccountReservationStatusQueryHandler(_apiClient.Object, _validator.Object,
                _configOptions.Object);
        }

        [Test]
        public async Task AndTheQueryIsValid_ThenTheReservationStatusIsReturned()
        {
            //Arrange
            var apiResponse = new AccountReservationStatusResponse()
            {
                CanAutoCreateReservations = true
            };
            _apiClient
                .Setup(x => x.Get<AccountReservationStatusResponse>(It.IsAny<AccountReservationStatusRequest>()))
                .ReturnsAsync(apiResponse);
            var query = new GetAccountReservationStatusQuery {AccountId = 123456};
            _validator
                .Setup(x => x.ValidateAsync(It.IsAny<GetAccountReservationStatusQuery>()))
                .ReturnsAsync(new ValidationResult() {ValidationDictionary = new Dictionary<string, string>()});

            //Act
            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(apiResponse.CanAutoCreateReservations, result.CanAutoCreateReservations);
        }

        [Test]
        public void AndTheQueryIsNotValid_ThenValidationErrorThrown()
        {
            //Arrange
            _apiClient
                .Setup(x => x.Get<AccountReservationStatusResponse>(It.IsAny<AccountReservationStatusRequest>()))
                .ReturnsAsync(new AccountReservationStatusResponse());
            var query = new GetAccountReservationStatusQuery {AccountId = default(long)};
            _validator
                .Setup(x => x.ValidateAsync(It.IsAny<GetAccountReservationStatusQuery>()))
                .ReturnsAsync(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>()
                    {
                        {"test", "test132"}
                    }
                });

            //Act + Assert
            Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(query, CancellationToken.None)
            );
        }
    }
}