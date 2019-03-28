using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Courses.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses
{
    public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, GetCoursesResult>
    {
        private readonly IApiClient _apiClient;
        private readonly ReservationsApiConfiguration _options;

        public GetCoursesQueryHandler(IApiClient apiClient, IOptions<ReservationsApiConfiguration> options)
        {
            _apiClient = apiClient;
            _options = options.Value;
        }

        public async Task<GetCoursesResult> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
        {
            var apiRequest = new CoursesApiRequest(_options.Url);

            var result = await _apiClient.Get<GetCoursesResponse>(apiRequest);

            return new GetCoursesResult
            {
               Courses = new List<Course>(result.Courses)
            };
        }
    }
}
