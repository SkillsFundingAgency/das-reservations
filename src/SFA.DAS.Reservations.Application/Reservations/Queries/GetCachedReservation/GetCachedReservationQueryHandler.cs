using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation
{
    public class GetCachedReservationQueryHandler : IRequestHandler<GetCachedReservationQuery, GetCachedReservationResult>
    {
        private readonly IValidator<IReservationQuery> _validator;
        private readonly ICachedReservationRespository _cachedReservationRepository;

        public GetCachedReservationQueryHandler(IValidator<IReservationQuery> validator, ICachedReservationRespository cachedReservationRepository)
        {
            _validator = validator;
            _cachedReservationRepository = cachedReservationRepository;
        }

        public async Task<GetCachedReservationResult> Handle(GetCachedReservationQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            CachedReservation cachedReservation;

            if (request.UkPrn != default(uint))
            {
                cachedReservation =
                    await _cachedReservationRepository.GetProviderReservation(request.Id, request.UkPrn);
            }
            else
            {
                cachedReservation =
                    await _cachedReservationRepository.GetEmployerReservation(request.Id);
            }

            return new GetCachedReservationResult
            {
                Id = cachedReservation.Id,
                AccountId = cachedReservation.AccountId,
                AccountLegalEntityId = cachedReservation.AccountLegalEntityId,
                AccountLegalEntityName = cachedReservation.AccountLegalEntityName,
                AccountLegalEntityPublicHashedId = cachedReservation.AccountLegalEntityPublicHashedId,
                CourseId = cachedReservation.CourseId,
                CourseDescription = cachedReservation.CourseDescription,
                TrainingDate = cachedReservation.TrainingDate,
                AccountName = cachedReservation.AccountName,
                CohortRef = cachedReservation.CohortRef,
                EmployerHasSingleLegalEntity = cachedReservation.EmployerHasSingleLegalEntity,
                IsEmptyCohortFromSelect = cachedReservation.IsEmptyCohortFromSelect,
                UkPrn = cachedReservation.UkPrn
            };
        }
    }
}
