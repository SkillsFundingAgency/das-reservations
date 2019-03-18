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
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation
{
    public class GetReservationQueryHandler : IRequestHandler<GetReservationQuery, GetReservationResult>
    {
        private readonly IValidator<GetReservationQuery> _validator;
        private readonly IApiClient _apiClient;

        private ReservationsApiConfiguration _options;

        public GetReservationQueryHandler(IValidator<IReservationQuery> validator, IApiClient apiClient, IOptions<ReservationsApiConfiguration> options)
        {
            _validator = validator;
            _apiClient = apiClient;
            _options = options.Value;
        }

        public async Task<GetReservationResult> Handle(GetReservationQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var apiRequest = new ReservationApiRequest(
                _options.Url,
                null,
                null,
                DateTime.MinValue, 
                request.Id);

            var result = await _apiClient.Get<ReservationApiRequest, GetReservationResponse>(apiRequest);

            return new GetReservationResult
            {
                ReservationId = result.ReservationId,
                StartDate = result.StartDate,
                ExpiryDate = result.ExpiryDate,
                Course = result.Course
            };
        }
    }
}
