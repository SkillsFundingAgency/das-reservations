using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheReservationStartDateCommandHandler : IRequestHandler<CacheReservationStartDateCommand>
    {
        private readonly IValidator<CacheReservationStartDateCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;

        public CacheReservationStartDateCommandHandler(
            IValidator<CacheReservationStartDateCommand> validator, 
            ICacheStorageService cacheStorageService)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<Unit> Handle(CacheReservationStartDateCommand command, CancellationToken cancellationToken)
        {
            var queryValidationResult = await _validator.ValidateAsync(command);

            if (!queryValidationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", queryValidationResult.ErrorList), null, null);
            }

            var cachedReservation = await _cacheStorageService.RetrieveFromCache<CachedReservation>(command.Id.ToString());

            if (cachedReservation == null)
            {
                throw new Exception("No reservation was found with that Id");
            }

            cachedReservation.StartDate = command.StartDate;
            
            await _cacheStorageService.SaveToCache(command.Id.ToString(), cachedReservation, 1);
            return Unit.Value;
        }
    }
}