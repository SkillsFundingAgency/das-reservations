using System.Collections.Generic;

namespace SFA.DAS.Reservations.Domain.Courses.Api
{
    public class GetCoursesApiResponse
    {
        public IEnumerable<Course> Courses { get; set; }
    }
}
