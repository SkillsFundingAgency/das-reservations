using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.Encoding;

namespace SFA.DAS.Reservations.Web.Infrastructure;

public static class RouteValueKeys
{
    public const string ProviderId = "providerId";
    public const string AccountLegalEntityPublicHashedId = "employerAccountLegalEntityPublicHashedId";
    public const string CohortReference = "cohortReference";
    public const string ApprenticeshipId = "apprenticeshipHashedId";
}

public interface IAuthorizationValueProvider
{
    long GetApprenticeshipId();
    long GetCohortId();
    long? GetAccountLegalEntityId();
    long GetProviderId();
}

public class AuthorizationValueProvider(IHttpContextAccessor httpContextAccessor, IEncodingService encodingService) : IAuthorizationValueProvider
{
    public long GetApprenticeshipId()
    {
        return GetAndDecodeValueIfExists(RouteValueKeys.ApprenticeshipId, EncodingType.ApprenticeshipId);
    }

    public long GetCohortId()
    {
        return GetAndDecodeValueIfExists(RouteValueKeys.CohortReference, EncodingType.CohortReference);
    }
    
    public long? GetAccountLegalEntityId()
    {
        return FindAndDecodeValue(RouteValueKeys.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);
    }
    
    public long GetProviderId()
    {
        if (!TryGetValueFromHttpContext(RouteValueKeys.ProviderId, out var value))
        {
            return 0;
        }

        if (!int.TryParse(value, out var providerId))
        {
            providerId = 0;
        }
        
        return providerId;
    }
    
    private long GetAndDecodeValueIfExists(string keyName, EncodingType encodedType)
    {
        if (!TryGetValueFromHttpContext(keyName, out var encodedValue))
        {
            return 0;
        }

        if (!encodingService.TryDecode(encodedValue, encodedType, out var id))
        {
            throw new UnauthorizedAccessException($"Failed to decode '{keyName}' value '{encodedValue}' using encoding type '{encodedType}'");
        }

        return id;
    }
    
    private long? FindAndDecodeValue(string key, EncodingType encodingType)
    {
        if (!TryGetValueFromHttpContext(key, out var encodedValue))
        {
            return null;
        }

        if (!encodingService.TryDecode(encodedValue, encodingType, out var value))
        {
            throw new UnauthorizedAccessException($"Failed to decode '{key}' value '{encodedValue}' using encoding type '{encodingType}'");
        }

        return value;
    }
    
    private bool TryGetValueFromHttpContext(string key, out string value)
    {
        value = null;

        // for testing
        if (httpContextAccessor.HttpContext == null)
        {
            return false;
        }

        if (httpContextAccessor.HttpContext.GetRouteData().Values.TryGetValue(key, out var routeValue))
        {
            value = (string)routeValue;
        }
        else if (httpContextAccessor.HttpContext.Request.Query.TryGetValue(key, out var queryStringValue))
        {
            value = queryStringValue;
        }
        else if (httpContextAccessor.HttpContext.Request.HasFormContentType && httpContextAccessor.HttpContext.Request.Form.TryGetValue(key, out var formValue))
        {
            value = formValue;
        }

        return !string.IsNullOrWhiteSpace(value);
    }
}