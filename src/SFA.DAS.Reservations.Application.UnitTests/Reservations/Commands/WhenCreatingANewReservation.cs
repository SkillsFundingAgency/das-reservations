using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenCreatingANewReservation
    {
        private Mock<IValidator<ICreateReservationCommand>> _mockValidator;
        private Mock<IApiClient> _mockApiClient;
        private CreateReservationCommandHandler _commandHandler;
        private CreateReservationResponse _apiResponse;
        private Mock<IHashingService> _mockHashingService;
        private long _expectedAccountId;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _apiResponse = fixture.Create<CreateReservationResponse>();
            _expectedAccountId = fixture.Create<long>();

            _mockValidator = fixture.Freeze<Mock<IValidator<ICreateReservationCommand>>>();
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CreateReservationCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockApiClient = fixture.Freeze<Mock<IApiClient>>();
            _mockApiClient
                .Setup(client => client.Create<ReservationApiRequest, CreateReservationResponse>(It.IsAny<ReservationApiRequest>()))
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

            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c=>c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, AutoData]
        public async Task Then_Calls_Reservation_Api_To_Create_Reservation(
            CreateReservationCommand command)
        {
            command.StartDate = "2019-01";

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockApiClient.Verify(client => client.Create<ReservationApiRequest, CreateReservationResponse>(It.Is<ReservationApiRequest>(apiRequest => 
                apiRequest.AccountId == _expectedAccountId &&
                apiRequest.StartDate == "2019-Jan-01"))
                , Times.Once);
        }

        [Test, AutoData]
        public async Task Then_Returns_Response_From_Reservation_Api(
            CreateReservationCommand command)
        {
            command.StartDate = "2019-01";

            var result  = await _commandHandler.Handle(command, CancellationToken.None);

            result.Reservation.Id.Should().Be(_apiResponse.Id);
        }
    }
}