using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Models;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Queries
{
    public class GetCachedReservationQueryHandler : IRequestHandler<GetCachedReservationQuery, GetCachedReservationResult>
    {
        private readonly IValidator<IReservationQuery> _validator;
        private readonly ICacheStorageService _cacheService;

        public GetCachedReservationQueryHandler(IValidator<IReservationQuery> validator, ICacheStorageService cacheService)
        {
            _validator = validator;
            _cacheService = cacheService;
        }

        public async Task<GetCachedReservationResult> Handle(GetCachedReservationQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var reservation = await _cacheService.RetrieveFromCache<Reservation>(request.Id.ToString());

            if (reservation == null)
            {
                return new GetCachedReservationResult();
            }

            return new GetCachedReservationResult
            {
                ReservationId = reservation.Id,
                StartDate = reservation.StartDate
            };
        }
    }
}
