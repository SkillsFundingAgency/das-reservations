using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Web.Extensions;

namespace SFA.DAS.Reservations.Web.Infrastructure;

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
        if (!httpContextAccessor.HttpContext.TryGetValueFromHttpContext(RouteValueKeys.ProviderId, out var value))
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
        if (!httpContextAccessor.HttpContext.TryGetValueFromHttpContext(keyName, out var encodedValue))
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
        if (!httpContextAccessor.HttpContext.TryGetValueFromHttpContext(key, out var encodedValue))
        {
            return null;
        }

        if (!encodingService.TryDecode(encodedValue, encodingType, out var value))
        {
            throw new UnauthorizedAccessException($"Failed to decode '{key}' value '{encodedValue}' using encoding type '{encodingType}'");
        }

        return value;
    }
}