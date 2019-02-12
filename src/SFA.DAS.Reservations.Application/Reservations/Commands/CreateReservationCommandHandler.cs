using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.ReservationsApi;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IValidator<CreateReservationCommand> _validator;
        private readonly IOptions<ReservationsApiConfiguration> _apiOptions;
        private readonly IApiClient _apiClient;
        private readonly IHashingService _hashingService;

        public CreateReservationCommandHandler(
            IValidator<CreateReservationCommand> validator, 
            IOptions<ReservationsApiConfiguration> apiOptions,
            IApiClient apiClient,
            IHashingService hashingService)
        {
            _validator = validator;
            _apiOptions = apiOptions;
            _apiClient = apiClient;
            _hashingService = hashingService;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid())
            {
                throw new ArgumentException(
                    "The following parameters have failed validation", 
                    validationResult.ValidationDictionary
                        .Select(c => c.Key)
                        .Aggregate((item1, item2) => item1 + ", " + item2));
            }

            var apiRequest = new CreateReservationApiRequest (
                _apiOptions.Value.Url,
                _hashingService.DecodeValue, 
                command.AccountId, 
                command.StartDate);

            var reservationJson = await _apiClient.Create<CreateReservationApiRequest, CreateReservationApiResponse>(apiRequest);

            return new CreateReservationResult
            {
                Reservation = null
            };
        }
    }
}
