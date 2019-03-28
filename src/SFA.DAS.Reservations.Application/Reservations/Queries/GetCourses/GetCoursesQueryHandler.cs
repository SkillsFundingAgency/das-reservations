using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Courses.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses
{
    public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, GetCoursesResult>
    {
        private readonly IApiClient _apiClient;
        private readonly ICacheStorageService _cacheService;
        private readonly ReservationsApiConfiguration _options;

        public GetCoursesQueryHandler(IApiClient apiClient,
            IOptions<ReservationsApiConfiguration> options, ICacheStorageService cacheService)
        {
            _apiClient = apiClient;
            _cacheService = cacheService;
            _options = options.Value;
        }

        public async Task<GetCoursesResult> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
        {
            var cachedCourses = await _cacheService.RetrieveFromCache<GetCoursesResult>(nameof(GetCoursesQueryHandler));

            if (cachedCourses != null)
            {
                return cachedCourses;
            }
            
            var apiRequest = new CoursesApiRequest(_options.Url);

            var result = await _apiClient.Get<CoursesApiRequest, GetCoursesResponse>(apiRequest);

            var courseResult = new GetCoursesResult
            {
               Courses = new List<Course>(result.Courses)
            };

            await _cacheService.SaveToCache(nameof(GetCoursesQueryHandler), courseResult, 24);

            return courseResult;
        }
    }
}
