using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Courses.Api
{
    public class GetCoursesResponse
    {
        public IEnumerable<Course> Courses { get; set; }
    }
}
