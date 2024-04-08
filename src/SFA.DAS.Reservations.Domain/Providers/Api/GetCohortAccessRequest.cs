using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public record GetCohortAccessRequest : IGetApiRequest
{
    private readonly long _providerId;
    private readonly long _cohortId;

    public GetCohortAccessRequest(string apiBaseUrl, long providerId, long CohortId)
    {
        _providerId = providerId;
        _cohortId = CohortId;
        
        BaseUrl = apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/";
    }

    public string GetUrl => $"authorization/{_providerId}/can-access-cohort/{_cohortId}";
    public string BaseUrl { get; }
  
}