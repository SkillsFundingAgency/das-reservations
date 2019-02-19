using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenCreatingANewReservation
    {
        private Mock<IValidator<CreateReservationCommand>> _mockValidator;
        private Mock<IApiClient> _mockApiClient;
        private CreateReservationCommandHandler _commandHandler;
        private ReservationResponse _apiResponse;
        private Mock<IHashingService> _mockHashingService;
        private long _expectedAccountId;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _apiResponse = fixture.Create<ReservationResponse>();
            _expectedAccountId = fixture.Create<long>();

            _mockValidator = fixture.Freeze<Mock<IValidator<CreateReservationCommand>>>();
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CreateReservationCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockApiClient = fixture.Freeze<Mock<IApiClient>>();
            _mockApiClient
                .Setup(client => client.Create<CreateReservation, ReservationResponse>(It.IsAny<CreateReservation>()))
                .ReturnsAsync(_apiResponse);

            _mockHashingService = fixture.Freeze<Mock<IHashingService>>();
            _mockHashingService
                .Setup(service => service.DecodeValue(It.IsAny<string>()))
                .Returns(_expectedAccountId);

            _commandHandler = fixture.Create<CreateReservationCommandHandler>();
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Command(
            CreateReservationCommand command)
        {
            command.StartDate = "2019-01";

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Throws_ArgumentException(
            CreateReservationCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await _commandHandler.Handle(command, CancellationToken.None); };

            act.Should().ThrowExactly<ArgumentException>()
                .Which.ParamName.Contains(propertyName).Should().BeTrue();
        }

        [Test, AutoData]
        public async Task Then_Calls_Reservation_Api_To_Create_Reservation(
            CreateReservationCommand command)
        {
            command.StartDate = "2019-01";

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockApiClient.Verify(client => client.Create<CreateReservation, ReservationResponse>(It.Is<CreateReservation>(apiRequest => 
                apiRequest.AccountId == _expectedAccountId &&
                apiRequest.StartDate == new DateTime(2019,01,01)))
                , Times.Once);
        }

        [Test, AutoData]
        public async Task Then_Returns_Response_From_Reservation_Api(
            CreateReservationCommand command)
        {
            command.StartDate = "2019-01";

            var result  = await _commandHandler.Handle(command, CancellationToken.None);

            result.Reservation.Id.Should().Be(_apiResponse.ReservationId);
        }
    }
}