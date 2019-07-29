using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.TagHelpers
{
    public class ExternalUrlHelper : IExternalUrlHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ReservationsWebConfiguration _options;

        public ExternalUrlHelper(IOptions<ReservationsWebConfiguration> options, IConfiguration configuration)
        {
            _configuration = configuration;
            _options = options.Value;
        }

        public string GenerateUrl(UrlParameters urlParameters)
        {
            var baseUrl = _configuration["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase)
                ? _options.EmployerDashboardUrl
                : _options.DashboardUrl;

            return FormatUrl(baseUrl, urlParameters);
        }
        
        public string GenerateAddApprenticeUrl(Guid reservationId, string accountLegalEntityPublicHashedId, string courseId, uint ukPrn, DateTime? startDate, string cohortRef)
        {
            var queryString =
                $"?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}";

            if (startDate.HasValue)
            {
                queryString += $"&startMonthYear={startDate:MMyyyy}";
            }

            if (!string.IsNullOrWhiteSpace(courseId))
            {
                queryString += $"&courseCode={courseId}";
            }

            var controller = "unapproved";
            var action = "add-apprentice";
            if (!string.IsNullOrEmpty(cohortRef))
            {
                controller += $"/{cohortRef}";
                action = "apprentices/add";
            }

            var urlParameters = new UrlParameters
            {
                Id = ukPrn.ToString(),
                Controller = controller,
                Action = action,
                QueryString = queryString
            };

            return GenerateAddApprenticeUrl(urlParameters);
        }

        public string GenerateAddApprenticeUrl(UrlParameters urlParameters)
        {
            var baseUrl = _options.ApprenticeUrl;

            return FormatUrl(baseUrl, urlParameters);
        }

        private static string FormatUrl(string baseUrl, UrlParameters urlParameters)
        {
            var urlString = new StringBuilder();

            urlString.Append(FormatBaseUrl(baseUrl, urlParameters.SubDomain, urlParameters.Folder));

            if (!string.IsNullOrEmpty(urlParameters.Id))
            {
                urlString.Append($"{urlParameters.Id}/");
            }

            if (!string.IsNullOrEmpty(urlParameters.Controller))
            {
                urlString.Append($"{urlParameters.Controller}/");
            }

            if (!string.IsNullOrEmpty(urlParameters.Action))
            {
                urlString.Append($"{urlParameters.Action}/");
            }

            if (!string.IsNullOrEmpty(urlParameters.QueryString))
            {
                return $"{urlString.ToString().TrimEnd('/')}{urlParameters.QueryString}";
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
                returnUrl = $"{returnUrl}{folder}/";
            }

            return returnUrl;
        }
    }
}
