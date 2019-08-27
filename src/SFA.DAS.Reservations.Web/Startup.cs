using System;
using System.IO;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SFA.DAS.Authorization.CommitmentPermissions.DependencyResolution;
using SFA.DAS.Authorization.DependencyResolution;
using SFA.DAS.Authorization.Mvc.Extensions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.AzureConfigurationProvider;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.HealthCheck;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Authorization;
using SFA.DAS.Reservations.Web.StartupConfig;

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

            services.AddHealthChecks()
                .AddCheck<ApiHealthCheck>(
                    "Reservation Api",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] {"ready"});
            
            services.AddOptions();

            var isEmployerAuth = _configuration["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase);
            var isProviderAuth = _configuration["AuthType"].Equals("provider", StringComparison.CurrentCultureIgnoreCase);

            var serviceParameters = new ServiceParameters();
            if (isEmployerAuth)
            {
                serviceParameters.AuthenticationType = AuthenticationType.Employer;
            }
            else if (isProviderAuth)
            {
                serviceParameters.AuthenticationType = AuthenticationType.Provider;
            }

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
            services.AddAuthorization<AuthorizationContextProvider>();
            services.AddCommitmentPermissionsAuthorization();

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
                        options.AddAuthorization();
                    })
                .AddControllersAsServices()
                .AddSessionStateTempDataProvider()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = _configuration["Environment"] == "LOCAL" ? 5001 : 443;
                });

            services.AddSession(options => options.IdleTimeout = TimeSpan.FromHours(reservationsWebConfig.SessionTimeoutHours));

            services.AddMediatR(typeof(CreateReservationCommandHandler).Assembly);
            services.AddMediatRValidation();

            services.AddServices(serviceParameters);

            services.AddProviderRelationsApi(_configuration, _environment);

            services.AddApplicationInsightsTelemetry(_configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
			

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
                app.UseExceptionHandler("/error/500");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseDasHsts();
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

            app.UseHealthChecks();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
