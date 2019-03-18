using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses
{
    public class GetCoursesResult
    {
        public ICollection<Course> Courses { get; set; }
    }
}
