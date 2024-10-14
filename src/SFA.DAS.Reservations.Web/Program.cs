using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SFA.DAS.Reservations.Web;

public class Program
{
    public static void Main(string[] args)
    {
        ThreadPool.SetMinThreads(200, 200);
        CreateWebHostBuilder(args).Build().Run();
    }

    private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost
            .CreateDefaultBuilder(args)
            .UseApplicationInsights()
            .UseStartup<Startup>();
}