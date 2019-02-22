﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Models;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IValidator<CreateReservationCommand> _validator;
        private readonly IOptions<ReservationsApiConfiguration> _apiOptions;
        private readonly IApiClient _apiClient;
        private readonly IHashingService _hashingService;

        public CreateReservationCommandHandler(
            IValidator<CreateReservationCommand> validator, 
            IOptions<ReservationsApiConfiguration> apiOptions,
            IApiClient apiClient,
            IHashingService hashingService)
        {
            _validator = validator;
            _apiOptions = apiOptions;
            _apiClient = apiClient;
            _hashingService = hashingService;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);
            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var startDateComponents = command.StartDate.Split("-");
            var startYear = Convert.ToInt32(startDateComponents[0]);
            var startMonth = Convert.ToInt32(startDateComponents[1]);

            var apiRequest = new CreateReservation (
                _apiOptions.Value.Url,
                _hashingService.DecodeValue, 
                command.AccountId, 
                new DateTime(startYear, startMonth, 1));

            var response = await _apiClient.Create<CreateReservation, ReservationResponse>(apiRequest);

            return new CreateReservationResult
            {
                Reservation = new Reservation {Id = response.ReservationId}
            };
        }
    }
}
