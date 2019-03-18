using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Domain.Courses.Api
{
    public class CoursesApiRequest : BaseApiRequest
    {
        public override string CreateUrl { get; }
        public override string GetUrl => $"{BaseUrl}api/courses";

        public CoursesApiRequest(string baseUrl) : base(baseUrl)
        {
        }
    }
}
