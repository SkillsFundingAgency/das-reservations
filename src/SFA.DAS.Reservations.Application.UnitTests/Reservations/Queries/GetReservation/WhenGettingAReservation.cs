using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetReservation
{
    public class WhenGettingCourses
    {
        private GetReservationQueryHandler _handler;
        private Mock<IValidator<IReservationQuery>> _validator;
        private Mock<IApiClient> _apiClient;
        private Mock<IHashingService> _hashingService;
        private Mock<IOptions<ReservationsApiConfiguration>> _options;
        private readonly Guid _expectedReservationId = Guid.NewGuid();
        private const long ExpectedAccountId = 44321;
        private const string ExpectedHashedId = "TGF45";
        private const string ExpectedBaseUrl = "https://test.local/reservation/";
        private const string ExpectedLegalEntityName = "Test Legal Entity";
        private DateTime _expectedStartDate = DateTime.Now.AddDays(-20);
        private DateTime _expectedExpiryDate = DateTime.Now.AddDays(30);

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<IValidator<IReservationQuery>>();
            _validator.Setup(x => x.ValidateAsync(It.Is<GetReservationQuery>(c =>
                    c.Id.Equals(_expectedReservationId))))
                .ReturnsAsync(new ValidationResult());


            _apiClient = new Mock<IApiClient>();
            _apiClient.Setup(x =>
                    x.Get<ReservationApiRequest, GetReservationResponse>(
                        It.Is<ReservationApiRequest>(c =>
                            c.GetUrl.Equals(
                                $"{ExpectedBaseUrl}api/reservations/{_expectedReservationId}"))))
                .ReturnsAsync(new GetReservationResponse
                {
                    Id = _expectedReservationId,
                    StartDate = _expectedStartDate,
                    ExpiryDate = _expectedExpiryDate,
                    AccountLegalEntityName = ExpectedLegalEntityName
                });

            _hashingService = new Mock<IHashingService>();
            _hashingService.Setup(x => x.DecodeValue(ExpectedHashedId)).Returns(ExpectedAccountId);

            _options = new Mock<IOptions<ReservationsApiConfiguration>>();
            _options.Setup(x => x.Value.Url).Returns(ExpectedBaseUrl);

            _handler = new GetReservationQueryHandler(_validator.Object, _apiClient.Object, _options.Object);
        }

        [Test]
        public void Then_The_Command_Is_Validated_An_Exception_Thrown_If_Not_Valid()
        {
            //Arrange
            _validator
                .Setup(x => x.ValidateAsync(It.IsAny<GetReservationQuery>()))
                .ThrowsAsync(new ValidationException(
                    new System.ComponentModel.DataAnnotations.ValidationResult("Error", new List<string>()), null,
                    null));

            //Act Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _handler.Handle(new GetReservationQuery(), new CancellationToken()));
        }

        [Test]
        public async Task Then_The_Reservation_Is_Returned_By_Id()
        {
            //Arrange
            var command = new GetReservationQuery
            {
                Id = _expectedReservationId
            };

            //Act
            var actual = await _handler.Handle(command, new CancellationToken());

            //Assert
            Assert.AreEqual(_expectedReservationId, actual.ReservationId);
            Assert.AreEqual(_expectedStartDate, actual.StartDate);
            Assert.AreEqual(_expectedExpiryDate, actual.ExpiryDate);
            Assert.AreEqual(ExpectedLegalEntityName, actual.AccountLegalEntityName);
        }
    }
}