using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ReservationsApiConfiguration _config;
        private readonly IApiClient _apiClient;

        public ReservationService(IOptions<ReservationsApiConfiguration> configOptions, IApiClient apiClient)
        {
            _config = configOptions.Value;
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<Reservation>> GetReservations(long accountId)
        {
            var apiReservations = await _apiClient.GetAll<GetReservationResponse>(new ReservationApiRequest(_config.Url, accountId));

            var result = apiReservations.Select(apiReservation => new Reservation
            {
                Id = apiReservation.Id,
                AccountLegalEntityName = apiReservation.AccountLegalEntityName,
                AccountLegalEntityId = apiReservation.AccountLegalEntityId,
                CreatedDate = apiReservation.CreatedDate,
                StartDate = apiReservation.StartDate,
                ExpiryDate = apiReservation.ExpiryDate,
                IsExpired = apiReservation.IsExpired,
                Course = apiReservation.Course,
                Status = (ReservationStatus) apiReservation.Status,
                ProviderId = apiReservation.ProviderId
            });

            return result;
        }

        public async Task<CreateReservationResponse> CreateReservationLevyEmployer(Guid reservationId, long accountId, long accountLegalEntityId, long? transferSenderAccountId)
        {
            return await _apiClient.Create<CreateReservationResponse>(new ReservationApiRequest(
                _config.Url,
                reservationId,
                accountId,
                accountLegalEntityId,
                true,
                transferSenderAccountId
                ));
        }
    }
}