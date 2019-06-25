using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CreateReservationLevyEmployer
{
    [TestFixture]
    public class WhenCreatingANewReservationForLevyEmployer
    {
        private Mock<CreateReservationLevyEmployerCommand> _request;
        private CreateReservationLevyEmployerCommandHandler _handler;
        private Mock<IValidator<CreateReservationLevyEmployerCommand>> _validator;
        


        [SetUp]
        public void SetUp()
        {
            _request = new Mock<CreateReservationLevyEmployerCommand>();
            _validator = new Mock<IValidator<CreateReservationLevyEmployerCommand>>();
            _handler = new CreateReservationLevyEmployerCommandHandler(_validator.Object);    
        }

        [Test, MoqAutoData]
        public async Task ThenTheCommandGetsValidated()
        {
            //Act
            var result = await _handler.Handle(_request.Object, CancellationToken.None);

            //Assert
            _validator.Verify(x => x.ValidateAsync(_request.Object), Times.AtLeastOnce);
        }
    }
}
