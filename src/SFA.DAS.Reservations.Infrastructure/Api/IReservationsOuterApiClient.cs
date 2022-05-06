using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public interface IReservationsOuterApiClient
    {
        Task<TResponse> Get<TResponse>(IGetApiRequest request);
        Task<TResponse> GetWithRetry<TResponse>(IGetApiRequest request);
    }
}