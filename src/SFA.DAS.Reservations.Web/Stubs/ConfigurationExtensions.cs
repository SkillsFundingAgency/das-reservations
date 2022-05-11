using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Reservations.Web.Stubs
{
    public static class ConfigurationExtensions
    {
        public static bool UseStubs(this IConfiguration config)
        {
            return config.GetValue<bool>("UseStubs");
        }
    }
}