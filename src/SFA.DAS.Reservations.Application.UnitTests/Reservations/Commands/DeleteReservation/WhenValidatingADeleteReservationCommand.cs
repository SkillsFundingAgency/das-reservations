using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.DeleteReservation
{
    [TestFixture]
    public class WhenValidatingADeleteReservationCommand
    {
        [Test, AutoData]
        public async Task And_Empty_Guid_Then_Not_Valid(
            DeleteReservationCommandValidator validator)
        {
            var command = new DeleteReservationCommand();

            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Should().ContainKey(nameof(command.ReservationId));
        }

        [Test, AutoData]
        public async Task And_Real_Guid_Then_Is_Valid(
            DeleteReservationCommand command,
            DeleteReservationCommandValidator validator)
        {
            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
        }
    }
}