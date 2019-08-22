using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Commitments.Services;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Application.Providers.Services;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Repositories;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class ServiceRegistrationExtension
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IProviderPermissionsService, ProviderPermissionsService>();
            services.AddScoped<IExternalUrlHelper, ExternalUrlHelper>();
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<CommitmentsApiClient>();
            services.AddSingleton<IEncodingService, EncodingService>();
            services.AddSingleton<IProviderService, ProviderService>();
            services.AddTransient<ITrainingDateService, TrainingDateService>();
            services.AddTransient<ICourseService, CourseService>();
            services.AddTransient<IReservationService, ReservationService>();
            services.AddTransient<ICacheStorageService, CacheStorageService>();
            services.AddTransient<IFundingRulesService, FundingRulesService>();
            services.AddTransient<IReservationAuthorisationService, ReservationAuthorisationService>();

            services.AddTransient<ICommitmentService, CommitmentService>(provider => new CommitmentService(
                provider.GetService<CommitmentsApiClient>(),
                provider.GetService<IOptions<CommitmentsApiConfiguration>>()));

            services.AddTransient<ICachedReservationRespository, CachedReservationRepository>();
        }
    }
}
