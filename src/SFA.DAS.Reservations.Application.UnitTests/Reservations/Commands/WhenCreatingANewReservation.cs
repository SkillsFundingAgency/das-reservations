using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Infrastructure.Api;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    public class WhenCreatingANewReservation
    {
        [Test, MoqAutoData]
        public async Task Then_It_Validates_The_Command(
            CreateReservationCommand command,
            [Frozen] Mock<IValidator<CreateReservationCommand>> mockValidator,
            CreateReservationCommandHandler commandHandler)
        {
            mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult());

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

        [Test, Ignore("raison d'être")]
        public void Then_The_Reservation_Is_Mapped_To_The_Api_Type()
        {

        }
    }
}