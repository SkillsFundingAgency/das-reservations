using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Application.FundingRules.Services;
using SFA.DAS.Reservations.Application.Providers.Services;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Repositories;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class ServiceRegistrationExtension
    {
        public static void AddServices(this IServiceCollection services, ServiceParameters serviceParameters, IConfiguration configuration)
        {
            services.AddSingleton(serviceParameters);
            services.AddScoped<LevyNotPermittedFilter>();
            services.AddScoped<IProviderPermissionsService, ProviderPermissionsService>();
            services.AddScoped<IExternalUrlHelper, ExternalUrlHelper>();
            

            if (string.IsNullOrEmpty(configuration["IsIntegrationTest"]))
            {
                services.AddSingleton<IApiClient, ApiClient>();
                services.AddSingleton<IEncodingService, EncodingService>();    
            }
            
            services.AddSingleton<IProviderService, ProviderService>();
            services.AddTransient<ITrainingDateService, TrainingDateService>();
            services.AddSingleton<IUserClaimsService, UserClaimsService>();
            services.AddTransient<ICourseService, CourseService>();
            services.AddTransient<IReservationService, ReservationService>();
            services.AddTransient<ICacheStorageService, CacheStorageService>();
            services.AddTransient<IFundingRulesService, FundingRulesService>();
            services.AddTransient<IReservationAuthorisationService, ReservationAuthorisationService>();

            services.AddTransient<ICachedReservationRespository, CachedReservationRepository>();

            services.AddTransient(typeof(ISessionStorageService<>), typeof(SessionStorageService<>));

            services.AddApimClient<IReservationsService>((c,s) => new ReservationsService(c));
        }

        private static IServiceCollection AddApimClient<T>(
            this IServiceCollection serviceCollection,
            Func<HttpClient, IServiceProvider, T> instance) where T : class
        {
            serviceCollection.AddTransient(s =>
            {
                var settings = s.GetService<ReservationsOuterApiConfiguration>();

                var clientBuilder = new HttpClientBuilder()
                    .WithDefaultHeaders()
                    .WithApimAuthorisationHeader(settings)
                    .WithLogging(s.GetService<ILoggerFactory>());

                var httpClient = clientBuilder.Build();

                if (!settings.ApiBaseUrl.EndsWith("/"))
                    httpClient.BaseAddress = new Uri(settings.ApiBaseUrl + "/");
                else
                    httpClient.BaseAddress = new Uri(settings.ApiBaseUrl);

                return instance.Invoke(httpClient, s);
            });

            return serviceCollection;
        }
    }
}
