using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations
{
    public class GetReservationsQueryHandler : IRequestHandler<GetReservationsQuery, GetReservationsResult>
    {
        private readonly ReservationsApiConfiguration _config;
        private readonly IValidator<GetReservationsQuery> _validator;
        private readonly IApiClient _apiClient;

        public GetReservationsQueryHandler(IValidator<GetReservationsQuery> validator, IOptions<ReservationsApiConfiguration> configOptions, IApiClient apiClient)
        {
            _config = configOptions.Value;
            _validator = validator;
            _apiClient = apiClient;
        }

        public async Task<GetReservationsResult> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var apiReservations = await _apiClient.GetAll<GetReservationResponse>(new ReservationApiRequest(_config.Url, request.AccountId));
            
            var result = new GetReservationsResult
            {
                Reservations = apiReservations.Select(apiReservation => new Reservation
                {
                    Id = apiReservation.Id,
                    AccountLegalEntityName = apiReservation.AccountLegalEntityName,
                    AccountLegalEntityId = apiReservation.AccountLegalEntityId,
                    StartDate = apiReservation.StartDate,
                    ExpiryDate = apiReservation.ExpiryDate,
                    Course = apiReservation.Course,
                    Status = (ReservationStatus)apiReservation.Status,
                    ProviderId = apiReservation.ProviderId
                })
            };
            return result;
        }
    }
}