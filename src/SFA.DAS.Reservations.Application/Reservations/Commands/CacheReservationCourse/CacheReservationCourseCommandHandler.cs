using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Exceptions;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse
{
    public class CacheReservationCourseCommandHandler : IRequestHandler<CacheReservationCourseCommand, Unit>
    {
        private readonly IValidator<CacheReservationCourseCommand> _validator;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly ICachedReservationRespository _cachedReservationRepository;
        private readonly ICourseService _courseService;

        public CacheReservationCourseCommandHandler(
            IValidator<CacheReservationCourseCommand> validator, 
            ICacheStorageService cacheStorageService, 
            ICachedReservationRespository cachedReservationRepository,
            ICourseService courseService)
        {
            _validator = validator;
            _cacheStorageService = cacheStorageService;
            _cachedReservationRepository = cachedReservationRepository;
            _courseService = courseService;
        }

        public async Task<Unit> Handle(CacheReservationCourseCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            CachedReservation cachedReservation;

            if (!command.UkPrn.HasValue)
            {
                cachedReservation = await _cachedReservationRepository.GetEmployerReservation(command.Id);
            }
            else
            {
                cachedReservation = await _cachedReservationRepository.GetProviderReservation(command.Id, command.UkPrn.Value);
            }

            if (cachedReservation == null)
            {
                throw new CachedReservationNotFoundException(command.Id);
            }

            if (string.IsNullOrEmpty(command.SelectedCourseId))
            {
                var course = new Course(null,null,0);
                cachedReservation.CourseId = course.Id;
                cachedReservation.CourseDescription = course.CourseDescription;
            }
            else
            {
                var course = await _courseService.GetCourse(command.SelectedCourseId);

                cachedReservation.CourseId = course.Id;
                cachedReservation.CourseDescription = course.CourseDescription;
            }

            await _cacheStorageService.SaveToCache(cachedReservation.Id.ToString(), cachedReservation, 1);

            return Unit.Value;
        }
    }
}