using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands
{
    [TestFixture]
    public class WhenCachingANewReservation
    {
        private Mock<IValidator<BaseCreateReservationCommand>> _mockValidator;
        private CacheReservationCommandHandler _commandHandler;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _mockValidator = fixture.Freeze<Mock<IValidator<BaseCreateReservationCommand>>>();
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CacheReservationCommand>()))
                .ReturnsAsync(new ValidationResult());

            _commandHandler = fixture.Create<CacheReservationCommandHandler>();
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Command(
            CacheReservationCommand command)
        {
            command.StartDate = "2019-01";

            await _commandHandler.Handle(command, CancellationToken.None);

            _mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }
    }
}