using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Employers.Api
{
    public class GetTrustedEmployersRequest : IGetAllApiRequest
    {
        public string BaseUrl { get; }
        public string GetAllUrl { get; }

        public GetTrustedEmployersRequest ()
        {
            
        }
    }
}