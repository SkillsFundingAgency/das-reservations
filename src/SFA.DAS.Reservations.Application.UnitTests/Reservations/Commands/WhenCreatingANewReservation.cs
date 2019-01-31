using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
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
            await commandHandler.Handle(command, CancellationToken.None);

            mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, MoqAutoData, Ignore("raison d'être")]
        public void And_The_Command_Is_Not_Valid_Then_Throws_ArgumentException(
            CreateReservationCommand command,
            ValidationResult validationResult,
            string propertyName,
            [Frozen] Mock<IValidator<CreateReservationCommand>> mockValidator,
            CreateReservationCommandHandler commandHandler)
        {
            validationResult.AddError(propertyName);
            var result = commandHandler.Handle(command, CancellationToken.None);
        }

        

        [Test, Ignore("raison d'être")]
        public void Then_The_Reservation_Is_Mapped_To_The_Api_Type()
        {

        }
    }
}