using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectCourseViewModel
    {
        public IEnumerable<CourseViewModel> Courses { get; set; }
        public string CourseId { get; set; }
    }
}
