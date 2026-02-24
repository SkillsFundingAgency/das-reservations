using System;
using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Courses
{
    public class Course
    {
        public Course(string id, string title, int level, string standardApprenticeshipType = null)
        {
            Title = SetDefaultTitleIfEmpty(title);
            Id = id;
            Level = level;
            LearningType = standardApprenticeshipType;
        }

        [JsonProperty("CourseId")]
        public string Id { get; }

        public string Title { get; }

        public int Level { get; }

        public string CourseDescription => Title.Equals("UNKNOWN", StringComparison.CurrentCultureIgnoreCase) ? Title : $"{Title} - Level {Level}";

        public string LearningType { get; }

        private static string SetDefaultTitleIfEmpty(string title)
        {
            return string.IsNullOrEmpty(title) ? "Unknown" : title;
        }
    }
}