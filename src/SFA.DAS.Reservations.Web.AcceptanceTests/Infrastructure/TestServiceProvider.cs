using System;
using System.Collections.Generic;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Hosting;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure
{
    public class TestServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public TestServiceProvider(string authType)
        {
            var serviceCollection = new ServiceCollection();
            var configuration = GenerateConfiguration(authType);

            var startup = new Startup(configuration, new TestHostEnvironment());

            startup.ConfigureServices(serviceCollection);
            serviceCollection.ConfigureTestServiceCollection(configuration, null);
            serviceCollection.AddTransient<IWebHostEnvironment, TestHostEnvironment>();
            RegisterControllers(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }



        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public static IConfigurationRoot GenerateConfiguration(string authType, bool isIntegrationTest = false)
        {
            var configSource = new MemoryConfigurationSource
            {
                InitialData = new[]
                {
                    new KeyValuePair<string, string>("ConfigurationStorageConnectionString", "UseDevelopmentStorage=true;"),
                    new KeyValuePair<string, string>("ConfigNames", "SFA.DAS.Reservations.Web,SFA.DAS.EmployerAccountAPI:AccountApi,SFA.DAS.ProviderRelationships.Api.ClientV2,SFA.DAS.Encoding,SFA.DAS.EmployerUrlHelper:EmployerUrlHelper"),
                    new KeyValuePair<string, string>("Environment", "DEV"),
                    new KeyValuePair<string, string>("Version", "1.0"),
                    new KeyValuePair<string, string>("UseStubs", "true"),
                    new KeyValuePair<string, string>("StubAuth", "true"),
                    new KeyValuePair<string, string>("AuthType", authType),
                    new KeyValuePair<string, string>("IsIntegrationTest", isIntegrationTest.ToString()),
                    new KeyValuePair<string, string>("ReservationsApi:url", "https://local.test.com"),
                    new KeyValuePair<string, string>("ReservationsWeb:DashboardUrl", $"https://{TestDataValues.DashboardUrl}"),
                    new KeyValuePair<string, string>("ReservationsWeb:EmployerDashboardUrl", $"https://{TestDataValues.EmployerDashboardUrl}"),
                    new KeyValuePair<string, string>("ReservationsWeb:EmployerApprenticeUrl", $"https://{TestDataValues.EmployerApprenticeUrl}"),
                    new KeyValuePair<string, string>("ReservationsWeb:FindApprenticeshipTrainingUrl", $"https://test"),
                    new KeyValuePair<string, string>("ReservationsWeb:ApprenticeshipFundingRulesUrl", $"https://test"),
                    new KeyValuePair<string, string>("ReservationsWeb:UseDfESignIn", "false"),
                    new KeyValuePair<string, string>("Identity:Scopes", "one two"),
                    new KeyValuePair<string, string>("Identity:ClientId", "test"),
                    new KeyValuePair<string, string>("Identity:ClientSecret", "test"),
                    new KeyValuePair<string, string>("Identity:ChangePasswordUrl", "test/{0}/"),
                    new KeyValuePair<string, string>("Identity:ChangeEmailUrl", "test/{0}/"),
                    new KeyValuePair<string, string>("Identity:BaseAddress", "https://test.identity"),
                    new KeyValuePair<string, string>("ReservationsOuterApi:ApiBaseUrl", "https://local.test.com"),
                    new KeyValuePair<string, string>("ReservationsOuterApi:SubscriptionKey", ""),
                    new KeyValuePair<string, string>("ReservationsOuterApi:Version", "1.0"),
                    new KeyValuePair<string, string>("ProviderRelationshipsApi:ApiBaseUrl", "https://local.test.com"),
                    new KeyValuePair<string, string>("CommitmentsApiClient:ApiBaseUrl", "https://local.test.com")
                }
            };

            var provider = new MemoryConfigurationProvider(configSource);

            return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
        }

        private static void RegisterControllers(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(sp =>
                new ReservationsController(
                    sp.GetService<IMediator>(),
                    sp.GetService<ITrainingDateService>(),
                    sp.GetService<IOptions<ReservationsWebConfiguration>>(),
                    sp.GetService<ILogger<ReservationsController>>(),
                    sp.GetService<IEncodingService>(),
                    sp.GetService<IExternalUrlHelper>()
                    )
                {
                    ControllerContext = GetControllerContext<ReservationsController>()
                });
            serviceCollection.AddTransient(sp =>
                new EmployerReservationsController(
                    sp.GetService<IMediator>(),
                    sp.GetService<IEncodingService>(),
                    sp.GetService<IOptions<ReservationsWebConfiguration>>(),
                    sp.GetService<IExternalUrlHelper>(),
                    sp.GetService<ILogger<EmployerReservationsController>>(),
                    sp.GetService<IUserClaimsService>()
                )
                {
                    ControllerContext = GetControllerContext<EmployerReservationsController>()
                });
            serviceCollection.AddTransient(sp =>
                new ProviderReservationsController(
                    sp.GetService<IMediator>(),
                    sp.GetService<IExternalUrlHelper>(),
                    sp.GetService<IEncodingService>(),
                    sp.GetService<ISessionStorageService<GetTrustedEmployersResponse>>()
                )
                {
                    ControllerContext = GetControllerContext<ProviderReservationsController>()
                });
            serviceCollection.AddTransient(sp =>
                new ManageReservationsController(
                    sp.GetService<IMediator>(),
                    sp.GetService<IEncodingService>(),
                    sp.GetService<IExternalUrlHelper>(),
                    sp.GetService<ISessionStorageService<ManageReservationsFilterModelBase>>(),
                    sp.GetService<ILogger<ManageReservationsController>>()
                    )
                {
                    ControllerContext = GetControllerContext<ManageReservationsController>()
                });

            serviceCollection.AddTransient(sp =>
                new SelectReservationsController(
                    sp.GetService<IMediator>(),
                    sp.GetService<ILogger<ReservationsController>>(),
                    sp.GetService<IEncodingService>(),

                    sp.GetService<IConfiguration>(),
                    sp.GetService<IExternalUrlHelper>(),
                    sp.GetService<IUserClaimsService>()
                )
                {
                    ControllerContext = GetControllerContext<SelectReservationsController>()
                });
        }

        private static ControllerContext GetControllerContext<T>() where T : ControllerBase
        {
            var controllerName = typeof(T).Name.Replace("Controller", "");

            var descriptor = new ControllerActionDescriptor
            {
                ControllerName = controllerName,
                ControllerTypeInfo = typeof(T).GetTypeInfo()
            };

            var httpContext = new DefaultHttpContext();
            var context = new ControllerContext(new ActionContext(httpContext, new RouteData(), descriptor));

            return context;
        }
    }

    public class TestHostEnvironment : IWebHostEnvironment
    {
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
        public string ApplicationName { get => "SFA.DAS.Reservations.Web"; set { } }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get => "Development"; set { } }
    }
    public class EmployerTestServiceProvider : TestServiceProvider
    {
        public EmployerTestServiceProvider() : base("employer")
        {
        }
    }

    public class ProviderTestServiceProvider : TestServiceProvider
    {
        public ProviderTestServiceProvider() : base("provider")
        {
        }
    }
}
