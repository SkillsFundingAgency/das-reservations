using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.TagHelpers;

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

    public string GenerateAddApprenticeUrl(Guid? reservationId, string accountLegalEntityPublicHashedId,
        string courseId, uint? ukPrn, DateTime? startDate, string cohortRef, string accountHashedId,
        bool isEmptyEmployerCohort = false, string transferSenderId = "",
        string encodedPledgeApplicationId= "", string journeyData = "", Guid? addApprenticeshipCacheKey = null)
    {
        var queryString = $"?reservationId={reservationId}";

        if (ukPrn.HasValue && !isEmptyEmployerCohort)
        {
            queryString += $"&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}";
        }
        else
        {
            queryString += $"&accountLegalEntityHashedId={accountLegalEntityPublicHashedId}";
        }
        if (ukPrn.HasValue && isEmptyEmployerCohort)
        {
            queryString += $"&providerId={ukPrn}";
        }

        if (startDate.HasValue)
        {
            queryString += $"&startMonthYear={startDate:MMyyyy}";
        }

        if (!string.IsNullOrWhiteSpace(courseId))
        {
            queryString += $"&courseCode={courseId}";
        }
            
        if (!string.IsNullOrWhiteSpace(journeyData))
        {
            queryString += $"&journeyData={journeyData}";
        }

        var isLevyAccount = string.IsNullOrWhiteSpace(courseId) && !startDate.HasValue;

        if (isLevyAccount)
        {
            queryString += "&autocreated=true";
        }

        if (!string.IsNullOrEmpty(transferSenderId))
        {
            queryString += $"&transferSenderId={transferSenderId}";
        }

        if (!string.IsNullOrEmpty(encodedPledgeApplicationId))
        {
            queryString += $"&encodedPledgeApplicationId={encodedPledgeApplicationId}";
        }

        if (addApprenticeshipCacheKey.HasValue)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                queryString = $"?addApprenticeshipCacheKey={addApprenticeshipCacheKey.Value}";
            }
            else
            {
                queryString += $"&addApprenticeshipCacheKey={addApprenticeshipCacheKey.Value}";
            }
        }

        string controller = "unapproved", action, id;
            
        if (ukPrn.HasValue && !isEmptyEmployerCohort)
        {
            action = "add/apprentice";
            id = ukPrn.ToString();
        }
        else if (ukPrn.HasValue)
        {          
            action = string.IsNullOrEmpty(courseId) ? "add/apprentice" : "add/select-delivery-model";
            id = accountHashedId;
        }
        else
        {
            action = "add";
            id = accountHashedId;
        }

        if (!string.IsNullOrEmpty(cohortRef))
        {
            controller += $"/{cohortRef}";
            action = !string.IsNullOrEmpty(courseId) && !string.IsNullOrEmpty(accountHashedId) 
                ? "apprentices/add/select-delivery-model" :"apprentices/add";
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

    public string GenerateCohortDetailsUrl(uint? ukprn, string accountId, string cohortRef, bool isEmptyCohort = false, 
        string journeyData = "", string accountLegalEntityHashedId = "", Guid? addApprenticeshipCacheKey = null)
    {
        var queryString = isEmptyCohort && ukprn.HasValue ? $"?providerId={ukprn}" : "";

        if (!string.IsNullOrWhiteSpace(journeyData))
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                queryString = $"?journeyData={journeyData}";
            }
            else
            {
                queryString += $"&journeyData={journeyData}";
            }
        }

        if (!string.IsNullOrWhiteSpace(accountLegalEntityHashedId))
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                queryString = $"?accountLegalEntityHashedId={accountLegalEntityHashedId}";
            }
            else
            {
                queryString += $"&accountLegalEntityHashedId={accountLegalEntityHashedId}";
            }
        }

        if (addApprenticeshipCacheKey.HasValue)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                queryString = $"?addApprenticeshipCacheKey={addApprenticeshipCacheKey.Value}";
            }
            else
            {
                queryString += $"&addApprenticeshipCacheKey={addApprenticeshipCacheKey.Value}";
            }
        }

        var urlParameters = new UrlParameters
        {
            Id = ukprn.HasValue && !isEmptyCohort ? ukprn.Value.ToString() : accountId,
            Controller = "unapproved",
            Action = isEmptyCohort ? "add/assign" : string.IsNullOrEmpty(accountId) ? $"{cohortRef}/details" : cohortRef,
            Folder = "",
            QueryString = queryString
        };

        return GenerateAddApprenticeUrl(urlParameters);
    }

    public string GenerateConfirmEmployerUrl(uint ukprn, string employerAccountLegalEntityPublicHashedId)
    {
        var queryString = $"?employerAccountLegalEntityPublicHashedId={employerAccountLegalEntityPublicHashedId}";

        var urlParameters = new UrlParameters
        {
            Id = ukprn.ToString(),
            Controller = "unapproved/add",
            Action = "confirm-employer",
            Folder = "",
            QueryString = queryString
        };

        return FormatUrl(_options.ApprenticeUrl, urlParameters);
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