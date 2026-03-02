using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Rules.Api
{
    public class GetAvailableDatesApiRequest(string baseUrl, long accountLegalEntityId, string courseId = null)
        : IGetApiRequest
    {
        public string BaseUrl { get; } = baseUrl;
        private long AccountLegalEntityId { get; } = accountLegalEntityId;
        private string CourseId { get; } = courseId;

        public string GetUrl => string.IsNullOrEmpty(CourseId)
            ? $"{BaseUrl}/rules/available-dates/{AccountLegalEntityId}"
            : $"{BaseUrl}/rules/available-dates/{AccountLegalEntityId}?courseId={System.Uri.EscapeDataString(CourseId)}";
    }
}