using System.Text;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.TagHelpers
{
    public class ProviderExternalUrlHelper : IExternalUrlHelper
    {
        private readonly ReservationsWebConfiguration _options;

        public ProviderExternalUrlHelper(IOptions<ReservationsWebConfiguration> options)
        {
            _options = options.Value;
        }

        public string GenerateUrl(string id="", string controller="", string action = "", string subDomain = "")
        {
            var urlString = new StringBuilder();
            urlString.Append(FormatBaseUrl(_options.DashboardUrl, subDomain));

            if (!string.IsNullOrEmpty(id))
            {
                urlString.Append($"{id}/");
            }

            if (!string.IsNullOrEmpty(controller))
            {
                urlString.Append($"{controller}/");
            }

            if (!string.IsNullOrEmpty(action))
            {
                urlString.Append($"{action}/");
            }

            return urlString.ToString().TrimEnd('/');
        }

        private static string FormatBaseUrl(string url, string subDomain = "")
        {
            var returnUrl = url.EndsWith("/")
                ? url
                : url + "/";

            if (!string.IsNullOrEmpty(subDomain))
            {
                returnUrl = returnUrl.Replace("https://", $"https://{subDomain}.");
            }

            return returnUrl;
        }
    }
}
