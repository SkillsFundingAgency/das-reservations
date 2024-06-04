using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Exceptions;
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
                .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });
            //Act
            Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));

            //Assert
            validator.Verify(x => x.ValidateAsync(request), Times.Once());

        }

        [Test, MoqAutoData]
        public void Then_Throws_A_TransferSender_Not_Allowed_Exception_If_Has_Failed_Validation_For_TransferSender(
            CreateReservationLevyEmployerCommand request,
            [Frozen] Mock<IValidator<CreateReservationLevyEmployerCommand>> validator,
            CreateReservationLevyEmployerCommandHandler handler
            )
        {
            //Arrange
            validator.Setup(x => x.ValidateAsync(It.IsAny<CreateReservationLevyEmployerCommand>()))
                .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>(), FailedTransferReceiverCheck = true });

            //Act
            Assert.ThrowsAsync<TransferSenderNotAllowedException>(() => handler.Handle(request, CancellationToken.None));

            //Assert
            validator.Verify(x => x.ValidateAsync(request), Times.Once());
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Account_Is_Not_A_Levy_Account_Then_A_Reservation_Is_Not_Created_And_Null_Returned(
            CreateReservationLevyEmployerCommand request,
            Guid id,
            [Frozen] Mock<IValidator<CreateReservationLevyEmployerCommand>> validator,
            [Frozen] Mock<IReservationService> service,
            CreateReservationLevyEmployerCommandHandler handler
            )
        {
            //Arrange
            request.TransferSenderId = null;
            validator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>(),
                    FailedAutoReservationCheck = true
                });
            service.Setup(x =>
                    x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId, request.TransferSenderId, request.UserId))
                .ReturnsAsync(new CreateReservationResponse { Id = id });

            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            service.Verify(x => x.CreateReservationLevyEmployer(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<Guid?>()), Times.Never);
            Assert.IsNull(result);
        }

        [Test, MoqAutoData]
        public void Then_If_The_Account_Is_Not_A_Levy_Account_And_The_Agreement_Is_Not_Signed_Then_An_Exception_Is_Thrown(
            CreateReservationLevyEmployerCommand request,
            Guid id,
            [Frozen] Mock<IValidator<CreateReservationLevyEmployerCommand>> validator,
            [Frozen] Mock<IReservationService> service,
            CreateReservationLevyEmployerCommandHandler handler
        )
        {
            //Arrange
            request.TransferSenderId = null;
            validator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>(),
                    FailedAutoReservationCheck = true,
                    FailedAgreementSignedCheck = true
                });
            service.Setup(x =>
                    x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId, request.TransferSenderId, request.UserId))
                .ReturnsAsync(new CreateReservationResponse { Id = id });

            //Act Assert
            Assert.ThrowsAsync<EmployerAgreementNotSignedException>(() =>
                handler.Handle(request, CancellationToken.None));
            service.Verify(x => x.CreateReservationLevyEmployer(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<Guid?>()), Times.Never);
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
                .ReturnsAsync(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>(),
                    FailedAutoReservationCheck = false,
                    FailedTransferReceiverCheck = false
                });

            service.Setup(x =>
                    x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId, request.TransferSenderId, request.UserId))
                .ReturnsAsync(new CreateReservationResponse { Id = id });

            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            service.Verify(x => x.CreateReservationLevyEmployer(It.IsAny<Guid>(), request.AccountId, request.AccountLegalEntityId, request.TransferSenderId, request.UserId));
            Assert.AreEqual(id, result.ReservationId);
        }

    }
}
