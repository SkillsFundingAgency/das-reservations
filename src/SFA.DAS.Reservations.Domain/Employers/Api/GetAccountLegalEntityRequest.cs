using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Employers.Api
{
    public class GetAccountLegalEntityRequest : IGetApiRequest
    {
        public long Id { get; }
        public string BaseUrl { get; }
        public string GetUrl => $"{BaseUrl}api/accountlegalentities/{Id}";

        public GetAccountLegalEntityRequest(string baseUrl, long id)
        {
            Id = id;
            BaseUrl = baseUrl;
        }
    }
}
