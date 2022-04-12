using System;
using System.IO;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using SFA.DAS.Authorization.DependencyResolution.Microsoft;
using SFA.DAS.Authorization.Mvc.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.HealthCheck;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Authorization;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.StartupConfig;

namespace SFA.DAS.Reservations.Web
{
    public class Startup
    {
        private const string EncodingConfigKey = "SFA.DAS.Encoding";
        
        private readonly IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _environment = environment;
            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
#endif
                .AddEnvironmentVariables();

            if (!configuration["Environment"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
            {
                config.AddAzureTableStorage(options =>
                    {
                        options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                        options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                        options.EnvironmentName = configuration["Environment"];
                        options.PreFixConfigurationKeys = false;
                        options.ConfigurationKeysRawJsonResult = new[] {EncodingConfigKey};
                    }
                );
            }
            
            _configuration = config.Build();
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

            var isEmployerAuth =
                _configuration["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase);
            var isProviderAuth =
                _configuration["AuthType"].Equals("provider", StringComparison.CurrentCultureIgnoreCase);

            var serviceParameters = new ServiceParameters();
            if (isEmployerAuth)
            {
                serviceParameters.AuthenticationType = AuthenticationType.Employer;
            }
            else if (isProviderAuth)
            {
                serviceParameters.AuthenticationType = AuthenticationType.Provider;
            }

            services.AddServices(serviceParameters,_configuration);
            
            if (_configuration["Environment"] != "DEV" || (
                !string.IsNullOrEmpty(_configuration["IsIntegrationTest"])
                && _configuration["IsIntegrationTest"].Equals("true",StringComparison.CurrentCultureIgnoreCase)))
            {
                if (isEmployerAuth)
                {
                    services.AddEmployerConfiguration(_configuration, _environment);
                }
                else if (isProviderAuth)
                {
                    services.AddProviderConfiguration(_configuration, _environment);
                }
            }

            var serviceProvider = services.BuildServiceProvider();

            services.AddAuthorizationService();
            services.AddAuthorization<AuthorizationContextProvider>();

            services.AddCommitmentsPermissionsApi(_configuration, _environment);
           
            if (isEmployerAuth)
            {
                services.AddAndConfigureEmployerAuthentication(
                    serviceProvider.GetService<IOptions<IdentityServerConfiguration>>(),
                    serviceProvider.GetService<IEmployerAccountService>());
            }

            if (isProviderAuth)
            {
                services.AddAndConfigureProviderAuthentication(
                    serviceProvider.GetService<IOptions<ProviderIdamsConfiguration>>(),
                    _configuration, 
                    _environment);
            }

            services.Configure<IISServerOptions>(options => { options.AutomaticAuthentication = false; });

            var reservationsWebConfig = serviceProvider.GetService<ReservationsWebConfiguration>();
            
            services.AddMvc(
                    options =>
                    {
                        options.Filters.Add(new GoogleAnalyticsFilter(serviceParameters));
                        options.AddAuthorization();
                    })
                .AddControllersAsServices()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = _configuration["Environment"] == "LOCAL" ? 5001 : 443;
            });

            services.AddMediatR(typeof(CreateReservationCommandHandler).Assembly);
            services.AddMediatRValidation();

            

            services.AddApplicationInsightsTelemetry(_configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            services.AddCommitmentsApi(_configuration, _environment);
            services.AddProviderRelationsApi(_configuration, _environment);

            if (_configuration["Environment"] == "LOCAL" || _configuration["Environment"] == "DEV")
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
            
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
            });

            services.AddDataProtection(_configuration, _environment);

            if (!_environment.IsDevelopment())
            {
                services.AddHealthChecks()
                    .AddCheck<ReservationsApiHealthCheck>(
                        "Reservation Api",
                        failureStatus: HealthStatus.Unhealthy,
                        tags: new[] {"ready"})
                    .AddCheck<ProviderRelationshipsApiHealthCheck>(
                        "ProviderRelationships Api",
                        failureStatus: HealthStatus.Unhealthy,
                        tags: new[] {"ready"})
                    .AddCheck<AccountApiHealthCheck>(
                        "Accounts Api",
                        failureStatus: HealthStatus.Unhealthy,
                        tags: new[] { "ready" });
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
                app.UseExceptionHandler("/error/500");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseDasHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
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

            if (!_environment.IsDevelopment())
            {
                app.UseHealthChecks();
            }

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}