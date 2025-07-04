using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Courses.Api;

public class GetCourseApiRequest(string baseUrl, string id) : IGetApiRequest
{
    public string BaseUrl { get; } = baseUrl;
    public string Id { get; } = id;
    public string GetUrl => $"{BaseUrl}api/courses/{Id}";
}