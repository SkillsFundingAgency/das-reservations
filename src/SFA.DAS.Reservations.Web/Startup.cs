using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Models.Configuration;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.EAS.Account.Api.Client;

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

            services.Configure<ReservationsConfiguration>(_configuration.GetSection("Reservations"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<ReservationsConfiguration>>().Value);
            services.Configure<IdentityServerConfiguration>(_configuration.GetSection("Identity"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<IdentityServerConfiguration>>().Value);
            services.Configure<Reservations.Models.Configuration.AccountApiConfiguration>(_configuration.GetSection("AccountApi"));
            services.AddSingleton<IAccountApiConfiguration,Reservations.Models.Configuration.AccountApiConfiguration>(cfg => cfg.GetService<IOptions<Reservations.Models.Configuration.AccountApiConfiguration>> ().Value);
            //AccountApiConfiguration
            services.AddTransient<IAccountApiClient, AccountApiClient>();
            services.AddTransient<IEmployerAccountService, EmployerAccountService>();

            var serviceProvider = services.BuildServiceProvider();
            var s5ervice = serviceProvider.GetService<IEmployerAccountService>();

            var config = serviceProvider.GetService<IOptions<IdentityServerConfiguration>>();
            services.AddAndConfigureAuthentication(config, serviceProvider.GetService<IEmployerAccountService>());
            services.AddAuthorizationService();
            services.AddMvc(
                    options => options.Filters.Add(new AuthorizeFilter(PolicyNames.HasEmployerAccount))
                    )
                .AddControllersAsServices()
                .AddSessionStateTempDataProvider()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSession(options => options.IdleTimeout = TimeSpan.FromHours(1));//todo: make configurable
            //todo: other dependent services here

            services.AddApplicationInsightsTelemetry(_configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
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
