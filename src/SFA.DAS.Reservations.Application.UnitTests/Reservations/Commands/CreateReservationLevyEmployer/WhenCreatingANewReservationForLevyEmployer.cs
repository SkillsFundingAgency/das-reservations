using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CreateReservationLevyEmployer
{
    [TestFixture]
    public class WhenCreatingANewReservationForLevyEmployer
    {
        [Test, MoqAutoData]
        public void ThenTheCommandGetsValidatedAndThrowsAnExceptionIfNotValid(
            CreateReservationLevyEmployerCommand request,
            [Frozen] Mock<IValidator<CreateReservationLevyEmployerCommand>> validator,
            CreateReservationLevyEmployerCommandHandler handler)
        {
            //Arrange
            validator.Setup(x => x.ValidateAsync(It.IsAny<CreateReservationLevyEmployerCommand>()))
                .ReturnsAsync(new ValidationResult {ValidationDictionary = new Dictionary<string, string> {{"", ""}}});
            //Act
            Assert.ThrowsAsync<ValidationException>(async() => await handler.Handle(request, CancellationToken.None));

            //Assert
            validator.Verify(x => x.ValidateAsync(request), Times.Once());
            
        }

        [Test, MoqAutoData]
        public async Task ThenCallsReservationServiceToCreateReservation(
            [Frozen]CreateReservationLevyEmployerCommand request,
            [Frozen] Mock<IValidator<CreateReservationLevyEmployerCommand>> validator,
            [Frozen] Mock<IReservationService> service,
            CreateReservationLevyEmployerCommandHandler handler)
        {
            //Arrange
            validator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult {ValidationDictionary = new Dictionary<string, string>()});

            service.Setup(x =>
                    x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId))
                .ReturnsAsync(new CreateReservationResponse());

            //Act
            var result = await handler.Handle(request, CancellationToken.None);
            
            //Assert
            service.Verify( x => x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId));

        }

        [Test, MoqAutoData]
        public async Task ThenReturnsCorrectResult(
            [Frozen] Guid id,
            [Frozen]CreateReservationLevyEmployerCommand request,
            [Frozen] Mock<IValidator<CreateReservationLevyEmployerCommand>> validator,
            [Frozen] Mock<IReservationService> service,
            CreateReservationLevyEmployerCommandHandler handler)
        {
            //Arrange
            validator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            service.Setup(x =>
                    x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId))
                .ReturnsAsync(new CreateReservationResponse(){Id = id});

            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.AreEqual(id,result.ReservationId);

        }

    }
}
