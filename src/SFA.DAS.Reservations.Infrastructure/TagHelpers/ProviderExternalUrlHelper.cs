using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.TagHelpers
{
    public class ProviderExternalUrlHelper : IExternalUrlHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ReservationsWebConfiguration _options;

        public ProviderExternalUrlHelper(IOptions<ReservationsWebConfiguration> options, IConfiguration configuration)
        {
            _configuration = configuration;
            _options = options.Value;
        }

        public string GenerateUrl(string id="", string controller="", string action = "", string subDomain = "", string folder="", string queryString="")
        {
            var urlString = new StringBuilder();

            var baseUrl = _configuration["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase)
                ? _options.EmployerDashboardUrl
                : _options.DashboardUrl;

            urlString.Append(FormatBaseUrl(baseUrl, subDomain, folder));

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

            if (!string.IsNullOrEmpty(queryString))
            {
                return $"{urlString.ToString().TrimEnd('/')}{queryString}";
            }

            return urlString.ToString().TrimEnd('/');
        }

        public string GenerateAddApprenticeUrl(uint? ukPrn, Guid reservationId, string accountLegalEntityPublicHashedId, DateTime startDate, string courseId)
        {
            var apprenticeUrl = $"{_options.ApprenticeUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}";
            if (!string.IsNullOrWhiteSpace(courseId))
            {
                apprenticeUrl += $"&courseCode={courseId}";
            }

            return apprenticeUrl;
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
