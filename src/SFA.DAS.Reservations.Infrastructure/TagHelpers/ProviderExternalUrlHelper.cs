using System.Text;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.TagHelpers
{
    public class ProviderExternalUrlHelper : IExternalUrlHelper
    {
        private readonly ReservationsWebConfiguration _options;

        public ProviderExternalUrlHelper(IOptions<ReservationsWebConfiguration> options)
        {
            _options = options.Value;
        }

        public string GenerateUrl(string id="", string controller="", string action = "", string subDomain = "", string folder="")
        {
            var urlString = new StringBuilder();
            urlString.Append(FormatBaseUrl(_options.DashboardUrl, subDomain, folder));

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

        private static string FormatBaseUrl(string url, string subDomain = "", string folder = "")
        {
            var returnUrl = url.EndsWith("/")
                ? url
                : url + "/";

            if (!string.IsNullOrEmpty(subDomain))
            {
                returnUrl = returnUrl.Replace("https://", $"https://{subDomain}.");
            }

            if (!string.IsNullOrEmpty(folder))
            {
                returnUrl += $"{folder}/";
            }

            return returnUrl;
        }
    }
}
