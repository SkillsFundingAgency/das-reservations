using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Reservations.Infrastructure.HealthCheck;

namespace SFA.DAS.Reservations.Web.StartupConfig
{
    public static class HstsStartup
    {
        public static IApplicationBuilder UseDasHsts(this IApplicationBuilder app)
        {
            var hostingEnvironment = app.ApplicationServices.GetService<IHostingEnvironment>();
            
            if (!hostingEnvironment.IsDevelopment())
            {
                app.UseHsts();
            }

            return app;
        }
        
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
            });
            
            app.UseHealthChecks("/ping", new HealthCheckOptions
            {
                Predicate = (_) => false
            });

            return app;
        }
    }
}
