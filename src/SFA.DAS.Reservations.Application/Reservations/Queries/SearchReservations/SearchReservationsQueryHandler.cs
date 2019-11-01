using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations
{
    public class SearchReservationsQueryHandler : IRequestHandler<SearchReservationsQuery, SearchReservationsResult>
    {
        private readonly IValidator<SearchReservationsQuery> _validator;
        private readonly IReservationService _reservationService;

        public SearchReservationsQueryHandler(
            IValidator<SearchReservationsQuery> validator,
            IReservationService reservationService)
        {
            _validator = validator;
            _reservationService = reservationService;
        }

        public async Task<SearchReservationsResult> Handle(SearchReservationsQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var serviceResult = await _reservationService.SearchReservations(request);

            return serviceResult;
        }
    }
}