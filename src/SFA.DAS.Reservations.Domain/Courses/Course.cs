using Newtonsoft.Json;
using System;
using System.Reflection.Metadata.Ecma335;

namespace SFA.DAS.Reservations.Domain.Courses
{
    public class Course
    {
        public Course(string id, string title, int level)
        {
            Title = SetDefaultTitleIfEmpty(title);
            Id = id;
            Level = level;

        }

        [JsonProperty("CourseId")]
        public string Id { get; }

        public string Title { get; }

        public int Level { get; }

        public string CourseDescription => Title.Equals("UNKNOWN",StringComparison.CurrentCultureIgnoreCase) ? Title : $"{Title} - Level {Level}";

        private static string SetDefaultTitleIfEmpty(string title)
        {
            return string.IsNullOrEmpty(title) ? "Unknown" : title;
        }
    }
}

