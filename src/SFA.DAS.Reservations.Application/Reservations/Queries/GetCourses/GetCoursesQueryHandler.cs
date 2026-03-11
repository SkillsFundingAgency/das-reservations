using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Application.Reservations.Services;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;

public class GetCoursesQueryHandler(ICourseService service) : IRequestHandler<GetCoursesQuery, GetCoursesResult>
{
    public async Task<GetCoursesResult> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var courses = await service.GetCourses();
            
        return new GetCoursesResult {Courses = courses};
    }
}