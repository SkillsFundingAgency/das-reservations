using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Models;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IValidator<CreateReservationCommand> _validator;
        private readonly IOptions<ReservationsApiConfiguration> _apiOptions;
        private readonly IApiClient _apiClient;

        public CreateReservationCommandHandler(
            IValidator<CreateReservationCommand> validator, 
            IOptions<ReservationsApiConfiguration> apiOptions,
            IApiClient apiClient)
        {
            _validator = validator;
            _apiOptions = apiOptions;
            _apiClient = apiClient;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var startDateComponents = command.StartDate.Split("-");
            var startYear = Convert.ToInt32(startDateComponents[0]);
            var startMonth = Convert.ToInt32(startDateComponents[1]);

            var apiRequest = new ReservationApiRequest(
                _apiOptions.Value.Url,
                command.AccountId, 
                new DateTime(startYear, startMonth, 1),
                command.Id,
                command.AccountLegalEntityName,
                command.CourseId);

            var response = await _apiClient.Create<ReservationApiRequest, CreateReservationResponse>(apiRequest);

            return new CreateReservationResult
            {
                Reservation = new Reservation {Id = response.Id}
            };
        }
    }
}
