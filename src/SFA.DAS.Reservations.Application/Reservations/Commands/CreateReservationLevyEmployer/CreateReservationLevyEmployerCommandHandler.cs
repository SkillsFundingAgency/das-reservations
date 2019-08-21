using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;


namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer
{
    public class CreateReservationLevyEmployerCommandHandler : IRequestHandler<CreateReservationLevyEmployerCommand, CreateReservationLevyEmployerResult>
    {
        private readonly IValidator<CreateReservationLevyEmployerCommand> _validator;
        private readonly IReservationService _reservationService;

        public CreateReservationLevyEmployerCommandHandler(
            IValidator<CreateReservationLevyEmployerCommand> validator,
            IReservationService reservationService)
        {
            _validator = validator;
            _reservationService = reservationService;
        }
        public async Task<CreateReservationLevyEmployerResult> Handle(CreateReservationLevyEmployerCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            if (validationResult.FailedTransferReceiverCheck)
            {
                throw new TransferSendNotAllowedException(request.AccountId, request.TransferSenderEmployerAccountId);
            }

            if (validationResult.FailedAutoReservationCheck)
            {
                return null;
            }

            var result = await _reservationService.CreateReservationLevyEmployer(Guid.NewGuid(), request.AccountId,
                request.AccountLegalEntityId, request.TransferSenderId);

            return new CreateReservationLevyEmployerResult
            {
                ReservationId = result.Id
            };
        }
    }
}
