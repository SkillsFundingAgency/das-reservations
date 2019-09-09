using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Reservations.Web.Stubs
{
    public static class ConfigurationExtensions
    {
        public static bool UseStub(this IConfiguration config)
        {
            var useStub = config.GetValue<bool>("UseStub");
            return useStub;
        }
    }
}