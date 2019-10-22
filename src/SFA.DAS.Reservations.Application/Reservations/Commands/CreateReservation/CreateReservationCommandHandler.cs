using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IValidator<CreateReservationCommand> _createReservationValidator;
        private readonly IValidator<CachedReservation> _cachedReservationValidator;
        private readonly IOptions<ReservationsApiConfiguration> _apiOptions;
        private readonly IApiClient _apiClient;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly ICachedReservationRespository _cachedReservationRepository;

        public CreateReservationCommandHandler(
            IValidator<CreateReservationCommand> createReservationValidator,
            IValidator<CachedReservation> cachedReservationValidator, 
            IOptions<ReservationsApiConfiguration> apiOptions,
            IApiClient apiClient,
            ICacheStorageService cacheStorageService,
            ICachedReservationRespository cachedReservationRepository)
        {
            _createReservationValidator = createReservationValidator;
            _cachedReservationValidator = cachedReservationValidator;
            _apiOptions = apiOptions;
            _apiClient = apiClient;
            _cacheStorageService = cacheStorageService;
            _cachedReservationRepository = cachedReservationRepository;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var queryValidationResult = await _createReservationValidator.ValidateAsync(command);

            if (!queryValidationResult.IsValid())
            {
                throw new ValidationException(queryValidationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            CachedReservation cachedReservation;

            if (!command.UkPrn.HasValue)
            {
                cachedReservation = await _cachedReservationRepository.GetEmployerReservation(command.Id);
            }
            else
            {
                cachedReservation = await _cachedReservationRepository.GetProviderReservation(command.Id, command.UkPrn.Value);
            }

            if (cachedReservation == null)
            {
                throw new CachedReservationNotFoundException(command.Id);
            }

            var createValidationResult = await _cachedReservationValidator.ValidateAsync(cachedReservation);
            if (!createValidationResult.IsValid())
            {
                throw new ValidationException(createValidationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var apiRequest = new ReservationApiRequest(
                _apiOptions.Value.Url,
                cachedReservation.AccountId, 
                cachedReservation.IsEmptyCohortFromSelect ? null : cachedReservation.UkPrn,
                new DateTime(cachedReservation.TrainingDate.StartDate.Year, cachedReservation.TrainingDate.StartDate.Month, 1),
                cachedReservation.Id,
                cachedReservation.AccountLegalEntityId,
                cachedReservation.AccountLegalEntityName,
                cachedReservation.CourseId,
                command.UserId);

            var response = await _apiClient.Create<CreateReservationResponse>(apiRequest);

            await _cacheStorageService.DeleteFromCache(command.Id.ToString());

            return new CreateReservationResult
            {
                Id = response.Id,
                AccountLegalEntityPublicHashedId = cachedReservation.AccountLegalEntityPublicHashedId,
                CohortRef = cachedReservation.CohortRef,
                IsEmptyCohortFromSelect = cachedReservation.IsEmptyCohortFromSelect,
                ProviderId = cachedReservation.UkPrn
            };
        }
    }
}
