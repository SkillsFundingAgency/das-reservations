using Newtonsoft.Json;
using System;

namespace SFA.DAS.Reservations.Domain.Courses
{
    public class Course
    {
        public Course(string id, string title, int level, string learningType = null)
        {
            Title = SetDefaultTitleIfEmpty(title);
            Id = id;
            Level = level;
            LearningType = learningType;
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
        public string LearningType { get; }
    }
}

