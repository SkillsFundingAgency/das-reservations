using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Providers.Api;

public record GetCohortAccessRequest : IGetApiRequest
{
    private readonly Party _party;
    private readonly long _partyId;
    private readonly long _cohortId;

    public string GetUrl => $"authorization/{_partyId}/can-access-cohort/{_cohortId}?party={_party}";
    public string BaseUrl { get; }

    public GetCohortAccessRequest(string baseUrl, Party party, long partyId, long cohortId)
    {
        _party = party;
        _partyId = partyId;
        _cohortId = cohortId;

        BaseUrl = baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";
    }
}

public record GetCohortAccessResponse
{
    public bool HasCohortAccess { get; set; }
}