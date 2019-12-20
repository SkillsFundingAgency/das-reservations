using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure
{
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public IConfigurationRoot ConfigurationRoot { get ; set ; }


        public TestWebApplicationFactory (string authType)
        {
            ConfigurationRoot =  TestServiceProvider.GenerateConfiguration(authType, true);
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return base.CreateWebHostBuilder().ConfigureAppConfiguration(c =>
            {
                c.AddConfiguration(ConfigurationRoot);
                c.AddEnvironmentVariables("ASPNETCORE");
            });
        }
    }
}