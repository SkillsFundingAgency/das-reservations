using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Employers.Api
{
    public class GetTrustedEmployersRequest : IGetAllApiRequest
    {
        public uint Id { get; }
        public string BaseUrl { get; }
        public string GetAllUrl => $"{BaseUrl}api/accountlegalentities/provider/{Id}";

        public GetTrustedEmployersRequest(string baseUrl, uint id)
        {
            Id = id;
            BaseUrl = baseUrl;
        }
    }
}