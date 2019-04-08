using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenValidatingACreateReservationCommand
    {
        [Test, AutoData]
        public async Task Then_If_Has_Id_Is_Valid(
            CreateReservationCommandValidator validator)
        {
            var command = new CreateReservationCommand
            {
                Id = Guid.NewGuid()
            };
           
            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
        }

        [Test, AutoData]
        public async Task Then_If_Has_No_Id_Is_Invalid(
            CreateReservationCommandValidator validator)
        {
            var command = new CreateReservationCommand();
           
            var result = await validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CreateReservationCommand.Id))
                .WhichValue.Should().Be($"{nameof(CreateReservationCommand.Id)} has not been supplied");
        }
    }
}
