﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations
{
    public class GetAvailableReservationsQueryHandler : IRequestHandler<GetAvailableReservationsQuery, GetAvailableReservationsResult>
    {
        private readonly IValidator<GetAvailableReservationsQuery> _validator;
        private readonly IReservationService _reservationService;

        public GetAvailableReservationsQueryHandler(IValidator<GetAvailableReservationsQuery> validator, IReservationService reservationService)
        {
            _validator = validator;
            _reservationService = reservationService;
        }

        public async Task<GetAvailableReservationsResult> Handle(GetAvailableReservationsQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var reservations = await _reservationService.GetReservations(request.AccountId);
            
            var result = new GetAvailableReservationsResult
            {
                Reservations = reservations
                    .Where(reservation => reservation.Status == ReservationStatus.Pending &&
                                          !reservation.IsExpired)
            };

            return result;
        }
    }
}