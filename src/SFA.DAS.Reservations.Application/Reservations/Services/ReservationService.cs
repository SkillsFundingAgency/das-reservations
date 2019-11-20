using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
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

        public async Task<SearchReservationsResponse> SearchReservations(SearchReservationsRequest filter)
        {
            var apiReservations = await _apiClient.Search<SearchReservationsApiResponse>(new SearchReservationsApiRequest(_config.Url, filter));

            var result = apiReservations.Reservations.Select(apiReservation => new Reservation
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

            return new SearchReservationsResponse
            {
                Reservations = result,
                NumberOfRecordsFound = apiReservations.NumberOfRecordsFound,
                EmployerFilters = apiReservations.Filters.EmployerFilters?.OrderBy(s => s).ToList() ?? new List<string>(),
                CourseFilters = apiReservations.Filters.CourseFilters?.OrderBy(s => s).ToList() ?? new List<string>(),
                StartDateFilters = SortStartDateFilters(apiReservations.Filters.StartDateFilters) ?? new List<string>()
            };
        }

        public async Task<CreateReservationResponse> CreateReservationLevyEmployer(Guid reservationId, long accountId,
            long accountLegalEntityId, long? transferSenderAccountId, Guid? userId)
        {
            return await _apiClient.Create<CreateReservationResponse>(new ReservationApiRequest(
                _config.Url,
                reservationId,
                accountId,
                accountLegalEntityId,
                true,
                transferSenderAccountId,
                userId
                ));
        }

        private IEnumerable<string> SortStartDateFilters(IEnumerable<string> startDateFilters)
        {
            if (startDateFilters == null)
                return new List<string>();

            var sortableFilters = new Dictionary<string, DateTime>();


            foreach (var startDateFilter in startDateFilters)
            {
                var firstDate = startDateFilter.Substring(0, 8);
                var date = DateTime.ParseExact(firstDate, "MMM yyyy", CultureInfo.InvariantCulture);
                sortableFilters.Add(startDateFilter, date);
            }

            return sortableFilters
                .OrderBy(pair => pair.Value)
                .Select(pair => pair.Key);
        }
    }
}