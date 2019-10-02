using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse
{
    public class CacheReservationCourseCommandValidator : IValidator<CacheReservationCourseCommand>
    {
        private readonly ICourseService _courseService;

        public CacheReservationCourseCommandValidator(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public async Task<ValidationResult> ValidateAsync(CacheReservationCourseCommand command)
        {
            var result = new ValidationResult();

            if (command.Id == Guid.Empty)
            {
                result.AddError(nameof(command.Id), $"{nameof( CacheReservationCourseCommand.Id)} has not been supplied");
            }

            if(string.IsNullOrEmpty(command.SelectedCourseId))
            {
                result.AddError(nameof(command.SelectedCourseId), "Select which apprenticeship training your apprentice will take");
            }
            else if(!await _courseService.CourseExists(command.SelectedCourseId))
            {
                result.AddError(nameof(command.SelectedCourseId), "Selected course does not exist");
            }

            return result;
        }
    }
}
