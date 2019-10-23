using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation
{
    public class DeleteReservationCommandHandler : IRequestHandler<DeleteReservationCommand>
    {
        private readonly IValidator<DeleteReservationCommand> _validator;
        private readonly IOptions<ReservationsApiConfiguration> _apiOptions;
        private readonly IApiClient _apiClient;

        public DeleteReservationCommandHandler(
            IValidator<DeleteReservationCommand> validator,
            IOptions<ReservationsApiConfiguration> apiOptions,
            IApiClient apiClient)
        {
            _validator = validator;
            _apiOptions = apiOptions;
            _apiClient = apiClient;
        }

        public async Task<Unit> Handle(DeleteReservationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    validationResult.ConvertToDataAnnotationsValidationResult(),
                    null, 
                    null);
            }

            var apiRequest = new ReservationApiRequest(_apiOptions.Value.Url, command.ReservationId, command.DeletedByEmployer);
            await _apiClient.Delete(apiRequest);

            return Unit.Value;
        }
    }
}