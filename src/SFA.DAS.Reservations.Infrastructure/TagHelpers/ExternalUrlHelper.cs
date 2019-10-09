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

        /// <summary>
        /// usage https://subDomain.baseUrl/folder/id/controller/action?queryString
        /// </summary>
        /// <param name="urlParameters"></param>
        /// <returns></returns>
        public string GenerateUrl(UrlParameters urlParameters)
        {
            var baseUrl = GetBaseUrl();

            return FormatUrl(baseUrl, urlParameters);
        }

        public string GenerateAddApprenticeUrl(Guid reservationId, string accountLegalEntityPublicHashedId, string courseId, uint? ukPrn, DateTime? startDate, string cohortRef, string accountHashedId)
        {
            var queryString = $"?reservationId={reservationId}";

            if (ukPrn.HasValue)
            {
                queryString += $"&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}";
            }
            else
            {
                queryString += $"&accountLegalEntityHashedId={accountLegalEntityPublicHashedId}";
            }

            if (startDate.HasValue)
            {
                queryString += $"&startMonthYear={startDate:MMyyyy}";
            }

            if (!string.IsNullOrWhiteSpace(courseId))
            {
                queryString += $"&courseCode={courseId}";
            }

            var isLevyAccount = string.IsNullOrWhiteSpace(courseId) && !startDate.HasValue;

            if (isLevyAccount)
            {
                queryString += "&autocreated=true";
            }

            string controller = "unapproved", action, id;
            
            if (ukPrn.HasValue)
            {
                action = "add/apprentice";
                id = ukPrn.ToString();
            }
            else
            {
                action = "add";
                id = accountHashedId;
            }

            if (!string.IsNullOrEmpty(cohortRef))
            {
                controller += $"/{cohortRef}";
                action = "apprentices/add";
            }

            var urlParameters = new UrlParameters
            {
                Id = id,
                Controller = controller,
                Action = action,
                QueryString = queryString
            };

            return GenerateAddApprenticeUrl(urlParameters);
        }


        /// <summary>
        /// usage https://subDomain.baseUrl/folder/id/controller/action?queryString
        /// </summary>
        /// <param name="urlParameters"></param>
        /// <returns></returns>
        public string GenerateAddApprenticeUrl(UrlParameters urlParameters)
        {
            var baseUrl = _configuration["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase)
                ? _options.EmployerApprenticeUrl
                : _options.ApprenticeUrl;

            return FormatUrl(baseUrl, urlParameters);
        }

        public string GenerateCohortDetailsUrl(uint? ukprn, string accountId, string cohortRef)
        {
            var urlParameters = new UrlParameters
            {
                Id = ukprn.HasValue ? ukprn.Value.ToString() : accountId,
                Controller = string.IsNullOrEmpty(cohortRef) ? "unapproved/add" : $"apprentices/{cohortRef}",
                Action = string.IsNullOrEmpty(cohortRef) ? "assign" : "details",
                Folder = ukprn.HasValue ? "" : "commitments/accounts"
            };

            var baseUrl = GetBaseUrl();

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

        private string GetBaseUrl()
        {
            return _configuration["AuthType"].Equals("employer", StringComparison.CurrentCultureIgnoreCase)
                ? _options.EmployerDashboardUrl
                : _options.DashboardUrl;
        }

        public string GenerateDashboardUrl(string accountId = null)
        {
            return string.IsNullOrEmpty(accountId)
                ? GenerateUrl(new UrlParameters {Controller = "Account"})
                : GenerateUrl(new UrlParameters
                {
                    Controller = "teams",
                    SubDomain = "accounts",
                    Folder = "accounts",
                    Id = accountId
                });
        }
    }
}
