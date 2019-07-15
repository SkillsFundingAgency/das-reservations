using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Commitments.Api
{
    public class GetCohortRequest : IGetApiRequest
    {
        public GetCohortRequest(string baseUrl, long cohortId)
        {
            BaseUrl = baseUrl;
            CohortId = cohortId;
        }
        public string BaseUrl { get; }
        public long CohortId { get; }
        public string GetUrl => $"{BaseUrl}api/cohorts/{CohortId}";
    }
}
