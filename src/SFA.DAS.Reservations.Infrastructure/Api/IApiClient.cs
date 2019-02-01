using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public interface IApiClient
    {
        Task<string> GetReservations(long accountId);
        Task<string> CreateReservation(long accountId, string json);
    }
}