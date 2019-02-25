using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Infrastructure.Api
{
    public interface IApiClient
    {
        Task<TResponse> Get<TRequest, TResponse>(TRequest request) where TRequest : BaseApiRequest;
        Task<TResponse> Create<TRequest, TResponse>(TRequest request) where TRequest : BaseApiRequest;
    }
}