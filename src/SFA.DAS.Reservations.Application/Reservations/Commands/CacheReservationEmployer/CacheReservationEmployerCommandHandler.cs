using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer
{
    public class CacheReservationEmployerCommandHandler : IRequestHandler<CacheReservationEmployerCommand, Unit>
    {
        private readonly IValidator<CacheReservationEmployerCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;

        public CacheReservationEmployerCommandHandler(IValidator<CacheReservationEmployerCommand> validator, ICacheStorageService cacheStorageService)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<Unit> Handle(CacheReservationEmployerCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            if (validationResult.FailedRuleValidation)
            {
                throw new ReservationLimitReachedException(command.AccountId);
            }

            if (validationResult.FailedAuthorisationValidation)
            {
                throw new ProviderNotAuthorisedException(command.AccountId, command.UkPrn.Value);
            }

            if (validationResult.FailedGlobalRuleValidation)
            {
                throw new GlobalReservationRuleException(command.AccountId);
            }

            if (validationResult.FailedEoiCheck)
            {
                throw new NonEoiUserAccessDeniedException(command.AccountId);
            }

            var reservation = new CachedReservation
            {
                Id = command.Id,
                AccountId = command.AccountId,
                AccountLegalEntityId = command.AccountLegalEntityId,
                AccountLegalEntityPublicHashedId = command.AccountLegalEntityPublicHashedId,
                AccountLegalEntityName = command.AccountLegalEntityName,
                AccountName = command.AccountName,
                CohortRef = command.CohortRef,
                UkPrn = command.UkPrn,
                EmployerHasSingleLegalEntity = command.EmployerHasSingleLegalEntity,
                IsEmptyCohortFromSelect = command.IsEmptyCohortFromSelect
            };

            await _cacheStorageService.SaveToCache(reservation.Id.ToString(), reservation, 1);

            return Unit.Value;
        }
    }
}
