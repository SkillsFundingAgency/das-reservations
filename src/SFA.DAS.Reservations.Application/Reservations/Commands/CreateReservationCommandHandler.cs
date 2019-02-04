using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Models;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IValidator<CreateReservationCommand> _validator;
        private readonly IApiClient _apiClient;
        private readonly IHashingService _hashingService;

        public CreateReservationCommandHandler(
            IValidator<CreateReservationCommand> validator, 
            IApiClient apiClient,
            IHashingService hashingService)
        {
            _validator = validator;
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

            var decodedAccountId = _hashingService.DecodeValue(command.AccountId);
            var apiRequest = new CreateReservationApiRequest
            {
                AccountId = decodedAccountId,
                StartDate = command.StartDate
            };

            var reservationJson = await _apiClient.CreateReservation(decodedAccountId, JsonConvert.SerializeObject(apiRequest));

            var reservation = JsonConvert.DeserializeObject<Reservation>(reservationJson);
            return new CreateReservationResult
            {
                Reservation = reservation
            };
        }
    }

    public interface IHashingService
    {
        long DecodeValue(string id);
    }
}
