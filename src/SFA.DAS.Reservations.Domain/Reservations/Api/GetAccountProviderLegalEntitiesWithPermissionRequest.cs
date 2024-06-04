using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Reservations.Api;
public class GetAccountProviderLegalEntitiesWithPermissionRequest : IGetApiRequest
{
    public GetAccountProviderLegalEntitiesWithPermissionRequest()
    {

    }
    public GetAccountProviderLegalEntitiesWithPermissionRequest(string baseUrl, List<Operation> operations, int ukprn, string accountHashedId = null, string accountLegalEntityPublicHashedId = null)
    {
        BaseUrl = baseUrl;
        Operations = operations;
        Ukprn = ukprn;
        AccountHashedId = accountHashedId;
        AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
    }
    public string AccountHashedId { get; set; }
    public string AccountLegalEntityPublicHashedId { get; set; }
    public int Ukprn { get; set; }
    public List<Operation> Operations { get; set; }
    public string BaseUrl { get; }
    public string GetUrl => $"{BaseUrl}AccountProviderLegalEntities";
}
