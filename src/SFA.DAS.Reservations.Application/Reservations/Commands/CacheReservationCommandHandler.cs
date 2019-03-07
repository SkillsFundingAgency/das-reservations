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
    public class CacheReservationCommandHandler : IRequestHandler<CacheReservationCommand, CacheReservationResult>
    {
        private readonly IValidator<BaseCreateReservationCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;

        public CacheReservationCommandHandler(IValidator<BaseCreateReservationCommand> validator, ICacheStorageService cacheStorageService)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<CacheReservationResult> Handle(CacheReservationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            await _cacheStorageService.SaveToCache(command.Id.ToString(), command, 1);
            
            return new CacheReservationResult{Id = Guid.Empty};
        }
    }
}