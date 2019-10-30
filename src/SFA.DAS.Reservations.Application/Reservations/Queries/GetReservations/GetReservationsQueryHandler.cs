using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations
{
    public class GetReservationsQueryHandler : IRequestHandler<GetReservationsQuery, GetReservationsResult>
    {
        private readonly IValidator<GetReservationsQuery> _validator;
        private readonly IReservationService _reservationService;

        public GetReservationsQueryHandler(IValidator<GetReservationsQuery> validator, IReservationService reservationService)
        {
            _validator = validator;
            _reservationService = reservationService;
        }

        public async Task<GetReservationsResult> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var reservations = await _reservationService.GetReservations(request.AccountId);
            
            var result = new GetReservationsResult
            {
                Reservations = reservations
            };

            return result;
        }
    }
}