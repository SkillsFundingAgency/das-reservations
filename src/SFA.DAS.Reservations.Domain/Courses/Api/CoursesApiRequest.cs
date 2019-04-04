using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Courses.Api
{
    public class CoursesApiRequest : IGetApiRequest
    {
        public CoursesApiRequest(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
        public string BaseUrl { get; }
        public string GetUrl => $"{BaseUrl}api/courses";

        
    }
}
