using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class CourseNotFoundException : Exception
    {
        public string _courseId { get; }

        public CourseNotFoundException(string courseId)
        {
            _courseId = courseId;
        }
    }
}
