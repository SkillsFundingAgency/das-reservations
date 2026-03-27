using System;
using Newtonsoft.Json;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.Reservations.Domain.Courses
{
    public class Course
    {
        public Course(string id, string title, int level, LearningType? learningType = null)
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

        public bool AllowPreviousDate { get; set; } = true;

        public string CourseDescription => Title.Equals("UNKNOWN",StringComparison.CurrentCultureIgnoreCase) ? Title : $"{Title} - Level {Level}";

        private static string SetDefaultTitleIfEmpty(string title)
        {
            return string.IsNullOrEmpty(title) ? "Unknown" : title;
        }
        public LearningType? LearningType { get; }
    }
}