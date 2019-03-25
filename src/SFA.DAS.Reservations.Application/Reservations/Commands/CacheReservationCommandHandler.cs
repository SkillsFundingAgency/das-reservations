using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheReservationCommandHandler : IRequestHandler<CacheCreateReservationCommand, CacheReservationResult>
    {
        private readonly IValidator<CacheCreateReservationCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;

        public CacheReservationCommandHandler(IValidator<CacheCreateReservationCommand> validator, ICacheStorageService cacheStorageService)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<CacheReservationResult> Handle(CacheCreateReservationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            if (!command.Id.HasValue)
            {
                command.Id = Guid.NewGuid();
            }

            await _cacheStorageService.SaveToCache(command.Id.ToString(), command, 1);
            
            return new CacheReservationResult{Id = command.Id.GetValueOrDefault()};
        }
    }
}