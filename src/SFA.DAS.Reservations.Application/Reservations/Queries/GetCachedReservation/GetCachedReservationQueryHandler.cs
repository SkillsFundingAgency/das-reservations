using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Api;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation
{
    public class GetCachedReservationQueryHandler(
        IValidator<IReservationQuery> validator,
        ICachedReservationRespository cachedReservationRepository,
        IReservationsOuterService outerApiService)
        : IRequestHandler<GetCachedReservationQuery, GetCachedReservationResult>
    {
        public async Task<GetCachedReservationResult> Handle(GetCachedReservationQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request);
            string apprenticeshipType = null;

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            CachedReservation cachedReservation;

            if (request.UkPrn != default(uint))
            {
                cachedReservation = await cachedReservationRepository.GetProviderReservation(request.Id, request.UkPrn);
            }
            else
            {
                cachedReservation =
                    await cachedReservationRepository.GetEmployerReservation(request.Id);
            }

            if (cachedReservation != null)
            {
                var courseDetail = await outerApiService.GetCourseDetails(cachedReservation.CourseId);
                apprenticeshipType = courseDetail?.ApprenticeshipType;
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
                ApprenticeshipType = apprenticeshipType,
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
