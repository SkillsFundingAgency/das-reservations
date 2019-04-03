using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse
{
    public class CacheReservationCourseCommandHandler : IRequestHandler<CacheReservationCourseCommand, Unit>
    {
        private readonly IValidator<CacheReservationCourseCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly ICourseService _courseService;

        public CacheReservationCourseCommandHandler(
            IValidator<CacheReservationCourseCommand> validator, 
            ICacheStorageService cacheStorageService, 
            ICourseService courseService)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
            _courseService = courseService;
        }

        public async Task<Unit> Handle(CacheReservationCourseCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(
                    new ValidationResult("The following parameters have failed validation", validationResult.ErrorList), null, null);
            }

            var cachedReservation = await _cacheStorageService.RetrieveFromCache<CachedReservation>(command.Id.ToString());

            if (cachedReservation == null)
            {
                throw new CachedReservationNotFoundException(command.Id);
            }

            var course = await _courseService.GetCourse(command.CourseId);

            cachedReservation.CourseId = course.Id;
            cachedReservation.CourseDescription = course.CourseDescription;

            await _cacheStorageService.SaveToCache(cachedReservation.Id.ToString(), cachedReservation, 1);

            return Unit.Value;
        }
    }
}