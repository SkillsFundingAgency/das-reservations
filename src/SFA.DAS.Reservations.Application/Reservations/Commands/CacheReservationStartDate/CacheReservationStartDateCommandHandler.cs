using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Exceptions;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate
{
    public class CacheReservationStartDateCommandHandler : IRequestHandler<CacheReservationStartDateCommand>
    {
        private readonly IValidator<CacheReservationStartDateCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly ICachedReservationRespository _cachedReservationRepository;

        public CacheReservationStartDateCommandHandler(
            IValidator<CacheReservationStartDateCommand> validator,
            ICacheStorageService cacheStorageService, 
            ICachedReservationRespository cachedReservationRepository)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
            _cachedReservationRepository = cachedReservationRepository;
        }

        public async Task Handle(CacheReservationStartDateCommand command, CancellationToken cancellationToken)
        {
            var queryValidationResult = await _validator.ValidateAsync(command);

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

            cachedReservation.TrainingDate = command.TrainingDate;
            
            await _cacheStorageService.SaveToCache(command.Id.ToString(), cachedReservation, 1);
        }
    }
}