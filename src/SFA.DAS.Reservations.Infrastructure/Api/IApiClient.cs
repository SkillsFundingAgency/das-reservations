using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public interface IApiClient
    {
        Task<TResponse> Get<TResponse>(IGetApiRequest request);
        Task<TResponse> GetAll<TResponse>(IGetAllApiRequest request);
        Task<TResponse> Create<TResponse>(IPostApiRequest request);
    }
}