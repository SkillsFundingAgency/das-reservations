using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectCourseViewModel
    {
        public IEnumerable<CourseViewModel> Courses { get; set; }
        public string CourseId { get; set; }
    }
}
