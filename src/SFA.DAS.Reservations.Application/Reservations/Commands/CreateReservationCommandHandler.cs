using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Models;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IValidator<IReservationQuery> _reservationQueryValidator;
        private readonly IValidator<ICreateReservationCommand> _createReservationValidator;
        private readonly IOptions<ReservationsApiConfiguration> _apiOptions;
        private readonly IApiClient _apiClient;
        private readonly IHashingService _hashingService;
        private readonly ICacheStorageService _cacheStorageService;

        public CreateReservationCommandHandler(
            IValidator<IReservationQuery> reservationQueryValidator,
            IValidator<ICreateReservationCommand> createReservationValidator, 
            IOptions<ReservationsApiConfiguration> apiOptions,
            IApiClient apiClient,
            IHashingService hashingService,
            ICacheStorageService cacheStorageService)
        {
            _reservationQueryValidator = reservationQueryValidator;
            _createReservationValidator = createReservationValidator;
            _apiOptions = apiOptions;
            _apiClient = apiClient;
            _hashingService = hashingService;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var queryValidationResult = await _reservationQueryValidator.ValidateAsync(command);
            if (!queryValidationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", queryValidationResult.ErrorList), null, null);
            }

            var reservation = await _cacheStorageService.RetrieveFromCache<GetCachedReservationResult>(command.Id.ToString());
            if (reservation == null)
            {
                throw new ValidationException(
                    new ValidationResult("No reservation was found with that Id"), null, null);
            }

            var createValidationResult = await _createReservationValidator.ValidateAsync(reservation);
            if (!createValidationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", createValidationResult.ErrorList), null, null);
            }

            var startDateComponents = reservation.StartDate.Split("-");
            var startYear = Convert.ToInt32(startDateComponents[0]);
            var startMonth = Convert.ToInt32(startDateComponents[1]);

            var apiRequest = new ReservationApiRequest(
                _apiOptions.Value.Url,
                _hashingService.DecodeValue, 
                reservation.AccountId, 
                new DateTime(startYear, startMonth, 1),
                reservation.Id,
                reservation.CourseId);

            var response = await _apiClient.Create<ReservationApiRequest, CreateReservationResponse>(apiRequest);

            await _cacheStorageService.DeleteFromCache(command.Id.ToString());

            return new CreateReservationResult
            {
                Reservation = new Reservation {Id = response.Id}
            };
        }
    }
}
