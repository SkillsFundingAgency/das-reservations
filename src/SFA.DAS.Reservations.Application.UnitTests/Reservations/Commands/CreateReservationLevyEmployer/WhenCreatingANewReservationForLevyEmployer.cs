using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            Assert.ThrowsAsync<ValidationException>(() =>  handler.Handle(request, CancellationToken.None));

            //Assert
            validator.Verify(x => x.ValidateAsync(request), Times.Once());
            
        }

        [Test, MoqAutoData]
        public async Task ThenCallsReservationServiceToCreateReservation(
            CreateReservationLevyEmployerCommand request,
            Guid id,
            [Frozen] Mock<IValidator<CreateReservationLevyEmployerCommand>> validator,
            [Frozen] Mock<IReservationService> service,
            CreateReservationLevyEmployerCommandHandler handler)
        {
            //Arrange
            request.TransferSenderId = null;
            validator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult {ValidationDictionary = new Dictionary<string, string>()});

            service.Setup(x =>
                    x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId, request.TransferSenderId))
                .ReturnsAsync(new CreateReservationResponse {Id = id});

            //Act
            var result = await handler.Handle(request, CancellationToken.None);
            
            //Assert
            service.Verify( x => x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId, request.TransferSenderId));
            Assert.AreEqual(id, result.ReservationId);
        }

    }
}
