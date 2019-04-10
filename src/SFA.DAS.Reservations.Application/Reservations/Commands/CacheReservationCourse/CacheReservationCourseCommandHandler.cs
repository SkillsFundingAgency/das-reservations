using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Exceptions;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse
{
    public class CacheReservationCourseCommandHandler : IRequestHandler<CacheReservationCourseCommand, Unit>
    {
        private readonly IValidator<CacheReservationCourseCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly ICachedReservationRespository _cachedReservationRespository;
        private readonly ICourseService _courseService;

        public CacheReservationCourseCommandHandler(
            IValidator<CacheReservationCourseCommand> validator, 
            ICacheStorageService cacheStorageService, 
            ICachedReservationRespository cachedReservationRespository,
            ICourseService courseService)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
            _cachedReservationRespository = cachedReservationRespository;
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

            if (string.IsNullOrEmpty(command.CourseId))
            {
                var course = new Course(null,null,0);
                cachedReservation.CourseId = course.Id;
                cachedReservation.CourseDescription = course.CourseDescription;
            }
            else
            {
                var course = await _courseService.GetCourse(command.CourseId);

                cachedReservation.CourseId = course.Id;
                cachedReservation.CourseDescription = course.CourseDescription;
            }

            await _cacheStorageService.SaveToCache(cachedReservation.Id.ToString(), cachedReservation, 1);

            return Unit.Value;
        }
    }
}