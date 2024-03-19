using System;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api;

public class GetAccountProviderLegalEntitiesWithPermissionRequest : IGetApiRequest
{
    private readonly long _ukprn;
    private readonly Operation _operation;
    
    // TODO: correct this Uri
    public string GetUrl => throw new NotImplementedException();
    public string BaseUrl { get; }

    public GetAccountProviderLegalEntitiesWithPermissionRequest(string apiBaseUrl, long ukprn, Operation operation)
    {
        _ukprn = ukprn;
        _operation = operation;
        
        BaseUrl = apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/";
    }
}