using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Courses.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public class CourseService : ICourseService
    {
        private readonly IApiClient _apiClient;
        private readonly ReservationsApiConfiguration _options;
        private readonly ICacheStorageService _cacheService;

        public CourseService(IApiClient apiClient, IOptions<ReservationsApiConfiguration> options, ICacheStorageService cacheService)
        {
            _apiClient = apiClient;
            _options = options.Value;
            _cacheService = cacheService;
        }

        public async Task<ICollection<Course>> GetCourses()
        {
            var coursesLookUp = await GetCachedLookup();

            return coursesLookUp.Values;
        }

        public async Task<Course> GetCourse(string id)
        {
            var coursesLookUp = await GetCachedLookup();

            if (!coursesLookUp.TryGetValue(id, out var course))
            {
                throw new CourseNotFoundException(id);
            }

            return course;
        }

        public async Task<bool> CourseExists(string id)
        {
            var coursesLookUp = await GetCachedLookup();

            return coursesLookUp.ContainsKey(id);
        }

        private async Task<IDictionary<string, Course>> GetCachedLookup()
        {
            var lookup = await _cacheService.RetrieveFromCache<IDictionary<string, Course>>(nameof(CourseService)) ?? await CacheCoursesFromApi();
            
            return lookup;
        }

        private async Task<IDictionary<string, Course>> CacheCoursesFromApi()
        {
            var apiRequest = new CoursesApiRequest(_options.Url);

            var result = await _apiClient.Get<CoursesApiRequest, GetCoursesResponse>(apiRequest);

            var coursesLookUp = result.Courses.ToDictionary(course => course.Id);

            await _cacheService.SaveToCache(nameof(CourseService), coursesLookUp, 24);
           
            return coursesLookUp;
        }

       
    }
}
