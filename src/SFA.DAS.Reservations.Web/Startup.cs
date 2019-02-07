using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Infrastructure.AzureConfigurationProvider;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Web.AppStart;

namespace SFA.DAS.Reservations.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddAzureTableStorageConfiguration(
                    builder["ConfigurationStorageConnectionString"],
                    builder["ConfigNames"].Split(","),
                    builder["Environment"],
                    builder["Version"]
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
            
            services.AddMvc(
                    options =>
                    {
                        options.Filters.Add(new AuthorizeFilter());
                    })
                .AddControllersAsServices()
                .AddSessionStateTempDataProvider()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSession(options => options.IdleTimeout = TimeSpan.FromHours(1));//todo: make configurable
            //todo: other dependent services here

            services.AddApplicationInsightsTelemetry(_configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
            services.AddSingleton(
                serviceProvider.GetService<ILoggerFactory>().CreateLogger("reservations-web"));//todo: what is this categoryName meant to be
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
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
