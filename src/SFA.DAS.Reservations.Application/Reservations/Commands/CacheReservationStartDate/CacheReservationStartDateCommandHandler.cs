﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate
{
    public class CacheReservationStartDateCommandHandler : IRequestHandler<CacheReservationStartDateCommand>
    {
        private readonly IValidator<CacheReservationStartDateCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly ICachedReservationRespository _cachedReservationRespository;

        public CacheReservationStartDateCommandHandler(
            IValidator<CacheReservationStartDateCommand> validator,
            ICacheStorageService cacheStorageService, 
            ICachedReservationRespository cachedReservationRespository)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
            _cachedReservationRespository = cachedReservationRespository;
        }

        public async Task<Unit> Handle(CacheReservationStartDateCommand command, CancellationToken cancellationToken)
        {
            var queryValidationResult = await _validator.ValidateAsync(command);

            if (!queryValidationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", queryValidationResult.ErrorList), null, null);
            }

            CachedReservation cachedReservation;

            if (command.UkPrn == default(uint))
            {
                cachedReservation = await _cachedReservationRespository.GetEmployerReservation(command.Id);
            }
            else
            {
                cachedReservation = await _cachedReservationRespository.GetProviderReservation(command.Id, command.UkPrn);
            }

            if (cachedReservation == null)
            {
                throw new CachedReservationNotFoundException(command.Id);
            }

            cachedReservation.StartDate = command.StartDate;
            cachedReservation.StartDateDescription = command.StartDateDescription;
            
            await _cacheStorageService.SaveToCache(command.Id.ToString(), cachedReservation, 1);
            return Unit.Value;
        }
    }
}