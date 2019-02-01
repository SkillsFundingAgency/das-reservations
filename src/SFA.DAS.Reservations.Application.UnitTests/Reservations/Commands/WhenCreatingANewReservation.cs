using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Models;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    public class WhenCreatingANewReservation
    {
        [Test, MoqAutoData]
        public async Task Then_It_Validates_The_Command(
            CreateReservationCommand command,
            [Frozen] Mock<IValidator<CreateReservationCommand>> mockValidator,
            [Frozen] Mock<IApiClient> mockApiClient,
            CreateReservationCommandHandler commandHandler)
        {
            mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult());

            mockApiClient
                .Setup(client => client.CreateReservation(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync("");

            await commandHandler.Handle(command, CancellationToken.None);

            mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, MoqAutoData]
        public void And_The_Command_Is_Not_Valid_Then_Throws_ArgumentException(
            CreateReservationCommand command,
            ValidationResult validationResult,
            string propertyName,
            [Frozen] Mock<IValidator<CreateReservationCommand>> mockValidator,
            CreateReservationCommandHandler commandHandler)
        {
            validationResult.AddError(propertyName);

            mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await commandHandler.Handle(command, CancellationToken.None); };

            act.Should().ThrowExactly<ArgumentException>()
                .Which.ParamName.Contains(propertyName).Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task Then_Calls_Reservation_Api_To_Create_Reservation(
            CreateReservationCommand command,
            [Frozen] Mock<IValidator<CreateReservationCommand>> mockValidator,
            [Frozen] Mock<IApiClient> mockApiClient,
            CreateReservationCommandHandler commandHandler)
        {
            var expectedJson = JsonConvert.SerializeObject(command);

            mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult());

            mockApiClient
                .Setup(client => client.CreateReservation(command.AccountId, expectedJson))
                .ReturnsAsync("");

            await commandHandler.Handle(command, CancellationToken.None);

            mockApiClient.Verify(client => client.CreateReservation(command.AccountId, expectedJson), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Response_From_Reservation_Api(
            CreateReservationCommand command,
            Reservation reservation,
            [Frozen] Mock<IValidator<CreateReservationCommand>> mockValidator,
            [Frozen] Mock<IApiClient> mockApiClient,
            CreateReservationCommandHandler commandHandler)
        {
            var commandJson = JsonConvert.SerializeObject(command);
            var reservationJson = JsonConvert.SerializeObject(reservation);

            mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult());

            mockApiClient
                .Setup(client => client.CreateReservation(command.AccountId, commandJson))
                .ReturnsAsync(reservationJson);

            var result  = await commandHandler.Handle(command, CancellationToken.None);

            result.Reservation.Should().BeEquivalentTo(reservation);
        }
    }
}