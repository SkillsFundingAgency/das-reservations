using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries
{
    public class WhenGettingAReservation
    {
        private GetReservationCommandHandler _handler;
        private Mock<IValidator<GetReservationCommand>> _validator;
        private Mock<IApiClient> _apiClient;
        private Mock<IHashingService> _hashingService;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private readonly Guid _expectedReservationId = Guid.NewGuid();
        private const long ExpectedAccountId = 44321;
        private const string ExpectedHashedId = "TGF45";
        private const string ExpectedBaseUrl = "https://test.local/reservation/";

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<IValidator<GetReservationCommand>>();
            _validator.Setup(x => x.ValidateAsync(It.Is<GetReservationCommand>(c =>
                    c.Id.Equals(_expectedReservationId) && c.AccountId.Equals(ExpectedHashedId))))
                .ReturnsAsync(new ValidationResult());


            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                    x.Get<ReservationApiRequest, GetReservationResponse>(
                        It.Is<ReservationApiRequest>(c =>
                            c.GetUrl.Equals(
                                $"{ExpectedBaseUrl}api/accounts/{ExpectedAccountId}/reservations/{_expectedReservationId}"))))
                .ReturnsAsync(new GetReservationResponse
                {
                    ReservationId = _expectedReservationId
                });

            _hashingService = new Mock<IHashingService>();
            _hashingService.Setup(x => x.DecodeValue(ExpectedHashedId)).Returns(ExpectedAccountId);

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(x => x.Value.Url).Returns(ExpectedBaseUrl);

            _handler = new GetReservationCommandHandler(_validator.Object, _apiClient.Object, _hashingService.Object,
                _options.Object);
        }

        [Test]
        public void Then_The_Command_Is_Validated_An_Exception_Thrown_If_Not_Valid()
        {
            //Arrange
            _validator
                .Setup(x => x.ValidateAsync(It.IsAny<GetReservationCommand>()))
                .ThrowsAsync(new ValidationException(
                    new System.ComponentModel.DataAnnotations.ValidationResult("Error", new List<string>()), null,
                    null));

            //Act Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _handler.Handle(new GetReservationCommand(), new CancellationToken()));
        }

        [Test]
        public async Task Then_The_Reservation_Is_Returned_By_Id()
        {
            //Arrange
            var command = new GetReservationCommand
            {
                Id = _expectedReservationId,
                AccountId = ExpectedHashedId
            };

            //Act
            var actual = await _handler.Handle(command, new CancellationToken());

            //Assert
            Assert.AreEqual(_expectedReservationId, actual.ReservationId);
            Assert.AreEqual(_expectedReservationId, actual.ReservationId);
        }
    }
}