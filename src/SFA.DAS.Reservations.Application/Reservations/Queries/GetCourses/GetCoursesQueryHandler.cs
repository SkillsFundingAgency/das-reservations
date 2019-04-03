using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Reservations.Services;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses
{
    public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, GetCoursesResult>
    {
        private readonly ICourseService _service;

        public GetCoursesQueryHandler(ICourseService service)
        {
            _service = service;
        }

        public async Task<GetCoursesResult> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
        {
            var courses = await _service.GetCourses();

            var result = await _apiClient.Get<CoursesApiRequest, GetCoursesResponse>(apiRequest);
            return new GetCoursesResult {Courses = courses};
        }
    }
}
