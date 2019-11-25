using System;
using System.Collections.Generic;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure
{
    public class TestServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public TestServiceProvider()
        {
            var authType = "employer";
            var serviceCollection = new ServiceCollection();
            var hosting = new HostingEnvironment{EnvironmentName = EnvironmentName.Development, ApplicationName = "SFA.DAS.Reservations.Web"};
;            var configuration = GenerateConfiguration(authType);

            var startup = new Startup(configuration, hosting);

            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode(It.Is<string>(s => s.Equals(TestDataValues.NonLevyHashedAccountId)),It.IsAny<EncodingType>())).Returns(TestDataValues.NonLevyAccountId);
            encodingService.Setup(x => x.Encode(It.Is<long>(l => l.Equals(TestDataValues.NonLevyAccountId)),It.IsAny<EncodingType>())).Returns(TestDataValues.NonLevyHashedAccountId);
            encodingService.Setup(x => x.Decode(It.Is<string>(s => s.Equals(TestDataValues.LevyHashedAccountId)),It.IsAny<EncodingType>())).Returns(TestDataValues.LevyAccountId);
            encodingService.Setup(x => x.Encode(It.Is<long>(l => l.Equals(TestDataValues.LevyAccountId)),It.IsAny<EncodingType>())).Returns(TestDataValues.LevyHashedAccountId);

            var apiClient = new Mock<IApiClient>();
            var accountApiClient = new Mock<IAccountApiClient>();

            var urlHelper = new Mock<IUrlHelper>();

            startup.ConfigureServices(serviceCollection);
            serviceCollection.AddTransient<IHostingEnvironment, HostingEnvironment>();
            serviceCollection.AddSingleton(encodingService.Object);
            serviceCollection.AddSingleton(apiClient.Object);
            serviceCollection.AddSingleton<IEmployerAccountService, EmployerAccountService>();
            serviceCollection.AddSingleton(accountApiClient.Object);
            serviceCollection.AddSingleton(urlHelper.Object);

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.Configure<ReservationsApiConfiguration>(configuration.GetSection("ReservationsApi"));
            serviceCollection.AddSingleton(config => config.GetService<IOptions<ReservationsApiConfiguration>>().Value);
            serviceCollection.Configure<ReservationsWebConfiguration>(configuration.GetSection("ReservationsWeb"));
            serviceCollection.AddSingleton(config => config.GetService<IOptions<ReservationsWebConfiguration>>().Value);
            RegisterControllers(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        private static IConfigurationRoot GenerateConfiguration(string authType)
        {
            var configSource = new MemoryConfigurationSource
            {
                InitialData = new[]
                {
                    new KeyValuePair<string, string>("ConfigurationStorageConnectionString", "UseDevelopmentStorage=true;"),
                    new KeyValuePair<string, string>("ConfigNames", "SFA.DAS.Reservations.Web,SFA.DAS.EmployerAccountAPI:AccountApi,SFA.DAS.ProviderRelationships.Api.ClientV2,SFA.DAS.Encoding"),
                    new KeyValuePair<string, string>("Environment", "DEV"),
                    new KeyValuePair<string, string>("Version", "1.0"),
                    new KeyValuePair<string, string>("UseStub", "true"),
                    new KeyValuePair<string, string>("AuthType", authType),
                    new KeyValuePair<string, string>("ReservationsApi:url", "https://local.test.com"),
                    new KeyValuePair<string, string>("ReservationsWeb:DashboardUrl", $"https://{TestDataValues.DashboardUrl}"),
                    new KeyValuePair<string, string>("ReservationsWeb:EmployerDashboardUrl", $"https://{TestDataValues.EmployerDashboardUrl}"),
                    new KeyValuePair<string, string>("ReservationsWeb:EmployerApprenticeUrl", $"https://{TestDataValues.EmployerApprenticeUrl}"),
                    new KeyValuePair<string, string>("ReservationsWeb:FindApprenticeshipTrainingUrl", $"https://test"),
                    new KeyValuePair<string, string>("ReservationsWeb:ApprenticeshipFundingRulesUrl", $"https://test")
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
                    sp.GetService<ILogger<EmployerReservationsController>>()
                )
                {
                    ControllerContext = GetControllerContext<EmployerReservationsController>()
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
                    sp.GetService<IExternalUrlHelper>()
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
}
