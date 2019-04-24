using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations
{
    public class GetReservationsQueryHandler : IRequestHandler<GetReservationsQuery, GetReservationsResult>
    {
        private readonly ReservationsApiConfiguration _config;
        private readonly IApiClient _apiClient;

        public GetReservationsQueryHandler(IOptions<ReservationsApiConfiguration> configOptions, IApiClient apiClient)
        {
            _config = configOptions.Value;
            _apiClient = apiClient;
        }

        public async Task<GetReservationsResult> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
        {
            //todo: validation - accountid not null and parsable to long
            
            var accountId = long.Parse(request.AccountId);
            var apiReservations = await _apiClient.GetAll<GetReservationResponse>(new ReservationApiRequest(_config.Url, accountId));
            
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
                    Status = apiReservation.Status,
                    ProviderId = apiReservation.ProviderId
                })
            };
            return result;
        }
    }
}