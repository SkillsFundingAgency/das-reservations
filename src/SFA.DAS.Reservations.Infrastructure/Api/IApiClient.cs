using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public interface IApiClient
    {
        Task<string> GetReservations();
    }
}