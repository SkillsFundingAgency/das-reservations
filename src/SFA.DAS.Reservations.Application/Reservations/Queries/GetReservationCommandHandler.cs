using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetReservationCommandHandler : IRequestHandler<GetReservationCommand, GetReservationResult>
    {
        private readonly IValidator<GetReservationCommand> _validator;
        private readonly IApiClient _apiClient;
        private readonly IHashingService _hashingService;
        private ReservationsApiConfiguration _options;

        public GetReservationCommandHandler(IValidator<GetReservationCommand> validator, IApiClient apiClient, IHashingService hashingService, IOptions<ReservationsApiConfiguration> options)
        {
            _validator = validator;
            _apiClient = apiClient;
            _hashingService = hashingService;
            _options = options.Value;
        }

        public async Task<GetReservationResult> Handle(GetReservationCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var apiRequest = new ReservationApiRequest(
                _options.Url,
                _hashingService.DecodeValue,
                request.AccountId,
                DateTime.MinValue, 
                request.Id);

            var result = await _apiClient.Get<ReservationApiRequest, GetReservationResponse>(apiRequest);

            return new GetReservationResult
            {
                ReservationId = result.ReservationId
            };
        }
    }
}
