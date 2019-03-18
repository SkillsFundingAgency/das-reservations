using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Web.Models
{
    public class CourseViewModel
    {
        public string Id { get; }
        public string Title { get; }
        public int Level { get; }
        public string Selected { get; }

        public CourseViewModel(Course course, string courseId = null)
        {
            Id = course.Id;
            Title = course.Title;
            Level = course.Level;
            Selected = Id.Equals(courseId, StringComparison.InvariantCulture)
                ? "selected"
                : null;
        }
    }
}