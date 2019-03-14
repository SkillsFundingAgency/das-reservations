using System;
using System.Reflection.Metadata.Ecma335;

namespace SFA.DAS.Reservations.Domain.Courses
{
    public class Course
    {
        public Course(string id, string title, int level)
        {
            Title = string.IsNullOrEmpty(title) ? "Unknown" : title;
            Id = id;
            Level = level;

        }

        public string Id { get; }
        public string Title { get; }
        public int Level { get; }
        public string CourseDescription => Title.Equals("UNKNOWN",StringComparison.CurrentCultureIgnoreCase) ? Title : $"{Title} - Level {Level}";
    }
}

