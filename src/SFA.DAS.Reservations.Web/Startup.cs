using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.IdentityModel.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Provider.Shared.UI.Startup;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.HealthCheck;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.StartupConfig;
using SFA.DAS.Reservations.Web.Infrastructure.ProviderSharedUi;

namespace SFA.DAS.Reservations.Web;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration.BuildDasConfiguration();
        _environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        IdentityModelEventSource.ShowPII = false;

        services.AddOptions();
        services.AddHttpContextAccessor();
        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
        });

        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        var serviceParameters = new ServiceParameters();

        if (_configuration.IsEmployerAuth())
        {
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
        }
        else if (_configuration.IsProviderAuth())
        {
            serviceParameters.AuthenticationType = AuthenticationType.Provider;
        }

        if (_configuration["Environment"] != "DEV" || (
                !string.IsNullOrEmpty(_configuration["IsIntegrationTest"])
                && _configuration["IsIntegrationTest"].Equals("true", StringComparison.CurrentCultureIgnoreCase)))
        {
            if (_configuration.IsEmployerAuth())
            {
                services.AddEmployerConfiguration(_configuration);
            }
            else if (_configuration.IsProviderAuth())
            {
                services.AddProviderConfiguration(_configuration);
            }
        }

        services.AddServices(serviceParameters, _configuration);
        services.AddScoped<SFA.DAS.Provider.Shared.UI.Extensions.IExternalUrlHelper, ProviderExternalUrlHelperAdapter>();

        services.AddAuthorizationServices();

        if (_configuration.IsEmployerAuth())
        {
            services.SetupEmployerAuth(_configuration);
        }

        if (_configuration.IsProviderAuth())
        {
            services.SetupProviderAuth(_configuration, _environment);
        }

        services.Configure<IISServerOptions>(options => { options.AutomaticAuthentication = false; });

        var reservationsWebConfig = _configuration
            .GetSection("ReservationsWeb")
            .Get<ReservationsWebConfiguration>();

        // Conditionally register Employer Shared UI services (required for employer layout and tag helpers)
        if (_configuration.IsEmployerAuth())
        {
            var resourceEnvironmentName = _configuration["ResourceEnvironmentName"] ?? _configuration["Environment"] ?? "LOCAL";
            services.AddMaMenuConfiguration(RouteNames.EmployerSignOut, resourceEnvironmentName);
        }

        // Register Provider Shared UI services when in provider mode
        if (_configuration.IsProviderAuth())
        {
            services.AddProviderUiServiceRegistration(_configuration);
        }

        // Register adapter for provider shared UI tag helpers (required because tag helper is imported globally)
        // This allows the provider tag helper to work in both employer and provider modes
        // Registered after AddProviderUiServiceRegistration so it overrides the package's registration
        services.AddScoped<SFA.DAS.Provider.Shared.UI.Extensions.IExternalUrlHelper, Infrastructure.ProviderSharedUi.ProviderExternalUrlHelperAdapter>();

        var mvcBuilder = services
            .AddMvc(options =>
            {
                options.Filters.Add(new GoogleAnalyticsFilter(serviceParameters));
                options.Conventions.Add(new KeepAliveControllerConvention(_configuration));
                
                // Add provider GaData filter when in provider mode
                if (_configuration.IsProviderAuth())
                {
                    options.Filters.Add(new Filters.ProviderGaDataFilter(serviceParameters));
                }
            })
            .AddControllersAsServices();

        // Configure provider shared UI navigation and filters when in provider mode
        if (_configuration.IsProviderAuth())
        {
            mvcBuilder
                .SetDefaultNavigationSection(SFA.DAS.Provider.Shared.UI.NavigationSection.Reservations)
                .EnableCookieBanner()
                .EnableGoogleAnalytics()
                .SuppressNavigationSection(SFA.DAS.Provider.Shared.UI.NavigationSection.StandardsAndTrainingVenues);
        }

        services.AddHttpsRedirection(options =>
        {
            options.HttpsPort = _configuration["Environment"] == "LOCAL" ? 5001 : 443;
        });

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(CreateReservationCommandHandler).Assembly));
        services.AddMediatRValidation();
        services.AddCommitmentsApi();

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
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.IsEssential = true;
        });

        services.AddDataProtection(reservationsWebConfig, _environment, _configuration.IsEmployerAuth());

        if (!_environment.IsDevelopment())
        {
            services.AddHealthChecks()
                .AddCheck<ReservationsApiHealthCheck>(
                    "Reservation Api",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["ready"]);
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
    }
}