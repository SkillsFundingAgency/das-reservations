using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SFA.DAS.Reservations.Web.CoreFix;

namespace SFA.DAS.Reservations.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
