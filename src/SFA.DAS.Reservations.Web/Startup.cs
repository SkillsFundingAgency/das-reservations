﻿using System;
using System.IO;
using System.Net.Http;
using HashidsNet;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Api.Client.Configuration;
using SFA.DAS.ProviderRelationships.Api.Client.Http;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Employers.Services;
using SFA.DAS.Reservations.Application.Reservations.Commands;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.AzureConfigurationProvider;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Stubs;
using HttpClientFactory = SFA.DAS.ProviderRelationships.Api.Client.Http.HttpClientFactory;

namespace SFA.DAS.Reservations.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _environment = environment;
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddAzureTableStorageConfiguration(
                    configuration["ConfigurationStorageConnectionString"],
                    configuration["ConfigNames"].Split(","),
                    configuration["Environment"],
                    configuration["Version"]
                    )
                .Build();

            _configuration = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            
            services.AddOptions();

            var isEmployerAuth = _configuration["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase);
            var isProviderAuth = _configuration["AuthType"].Equals("provider", StringComparison.CurrentCultureIgnoreCase);

            if (isEmployerAuth)
            {
                services.AddEmployerConfiguration(_configuration);
            }
            else if (isProviderAuth)
            {
                services.AddProviderConfiguration(_configuration);
            }

            var serviceProvider = services.BuildServiceProvider();

            services.AddAuthorizationService();

            if (isEmployerAuth)
            {
                services.AddAndConfigureEmployerAuthentication(serviceProvider.GetService<IOptions<IdentityServerConfiguration>>(), serviceProvider.GetService<IEmployerAccountService>());
            }

            if (isProviderAuth)
            {
                services.AddAndConfigureProviderAuthentication(serviceProvider.GetService<IOptions<ProviderIdamsConfiguration>>());
            }

            services.Configure<IISServerOptions>(options => { options.AutomaticAuthentication = false; });

            var reservationsWebConfig = serviceProvider.GetService<ReservationsWebConfiguration>();
            services.AddMvc(
                    options =>
                    {
                        options.Filters.Add(new AuthorizeFilter());
                        options.Filters.Add(new FeatureToggleActionFilter(_configuration));
                    })
                .AddControllersAsServices()
                .AddSessionStateTempDataProvider()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSession(options => options.IdleTimeout = TimeSpan.FromHours(reservationsWebConfig.SessionTimeoutHours));
            services.AddMediatR(typeof(CreateReservationCommandHandler).Assembly);
            services.AddScoped(typeof(IValidator<CreateReservationCommand>), typeof(CreateReservationCommandValidator));
            services.AddScoped(typeof(IValidator<CacheReservationCourseCommand>), typeof(CacheReservationCourseCommandValidator));
            services.AddScoped(typeof(IValidator<CacheReservationStartDateCommand>), typeof(CacheReservationStartDateCommandValidator));
            services.AddScoped(typeof(IValidator<CacheReservationEmployerCommand>), typeof(CacheReservationEmployerCommandValidator));
            services.AddScoped(typeof(IValidator<IReservationQuery>), typeof(GetReservationQueryValidator));
            services.AddScoped(typeof(IValidator<GetCachedReservationResult>), typeof(CachedReservationValidator));
            services.AddScoped(typeof(IValidator<GetTrustedEmployersQuery>), typeof(GetTrustedEmployerQueryValidator));
            services.AddScoped<IProviderPermissionsService,ProviderPermissionsService>();

            services.AddScoped<IExternalUrlHelper, ProviderExternalUrlHelper>();

            services.AddSingleton<IApiClient,ApiClient>();
            services.AddSingleton<IHashingService, HashingService>();
            services.AddSingleton<IHashids>(new Hashids(
                reservationsWebConfig.EmployerAccountHashSalt, 
                reservationsWebConfig.EmployerAccountHashLength, 
                reservationsWebConfig.EmployerAccountHashAlphabet));
            services.AddTransient<IStartDateService, StartDateService>();
            services.AddTransient<ICourseService, CourseService>();
            services.AddTransient<ICacheStorageService, CacheStorageService>();

            services.AddApplicationInsightsTelemetry(_configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            services.AddSingleton<ICurrentDateTime>(reservationsWebConfig.CurrentDateTime.HasValue
                ? new CurrentDateTime(reservationsWebConfig.CurrentDateTime)
                : new CurrentDateTime());
				
			AddProviderRelationsApi(services, _configuration, _environment);

            if (_configuration["Environment"] == "LOCAL")
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = reservationsWebConfig.RedisCacheConnectionString;
                    });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.None
            });

            app.Use(async (context, next) =>
            {
                if (context.Response.Headers.ContainsKey("X-Frame-Options"))
                {
                    context.Response.Headers.Remove("X-Frame-Options");
                }

                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");

                await next();

                if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                {
                    //Re-execute the request so the user gets the error page
                    var originalPath = context.Request.Path.Value;
                    context.Items["originalPath"] = originalPath;
                    context.Request.Path = "/error/404";
                    await next();
                }
            });
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static void AddProviderRelationsApi(IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                services.AddScoped<IProviderRelationshipsApiClient, ProviderRelationshipsApiClientStub>();
            }
            else
            {
                services.AddScoped<IProviderRelationshipsApiClient, ProviderRelationshipsApiClient>();
                services.AddScoped<IRestHttpClient, RestHttpClient>();

                services.AddSingleton<HttpClient>(provider =>
                    new HttpClientFactory(provider.GetService<ProviderRelationshipsApiClientConfiguration>()
                        .AzureActiveDirectoryClient).CreateHttpClient());

                services.Configure<AzureActiveDirectoryClientConfiguration>(configuration.GetSection("AzureActiveDirectoryClient"));
                services.AddSingleton(config => new ProviderRelationshipsApiClientConfiguration
                {
                    AzureActiveDirectoryClient = config.GetService<IOptions<AzureActiveDirectoryClientConfiguration>>().Value
                });
            }
        }
    }
}
