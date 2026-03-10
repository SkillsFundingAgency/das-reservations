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
        public string Description { get; }
        public bool AllowPreviousDate { get; }

        public CourseViewModel(Course course, string courseId = null)
        {
            if (course == null)
            {
                course = new Course(null,null,0);
                Description = course.CourseDescription;
                AllowPreviousDate = true;
                return;
            }

            Id = course.Id;
            Title = course.Title;
            Level = course.Level;
            Description = course.CourseDescription;
            AllowPreviousDate = course.AllowPreviousDate;
            Selected = Id!=null && Id.Equals(courseId, StringComparison.InvariantCulture)
                ? "selected"
                : null;
        }
    }
}