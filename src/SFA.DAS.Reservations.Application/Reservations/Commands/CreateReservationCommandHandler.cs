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

        public CreateReservationCommandHandler(IValidator<CreateReservationCommand> validator, IApiClient apiClient)
        {
            _validator = validator;
            _apiClient = apiClient;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ArgumentException(
                    "The following parameters have failed validation", 
                    validationResult.ValidationDictionary
                        .Select(c => c.Key)
                        .Aggregate((item1, item2) => item1 + ", " + item2));
            }

            var reservationJson = await _apiClient.CreateReservation(request.AccountId, JsonConvert.SerializeObject(request));

            var reservation = JsonConvert.DeserializeObject<Reservation>(reservationJson);

            return new CreateReservationResult
            {
                Reservation = reservation
            };
        }
    }
}
