using System.Net.Http;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Services
{
    public class ReservationsService : IReservationsService
    {
        private readonly HttpClient _client;

        public ReservationsService(HttpClient client)
        {
            _client = client;
        }
    }
}
