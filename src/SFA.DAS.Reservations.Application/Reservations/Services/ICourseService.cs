using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public interface ICourseService
    {
        Task<ICollection<Course>> GetCourses();
        Task<Course> GetCourse(string id);
        Task<bool> CourseExists(string id);
    }
}
