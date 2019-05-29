using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IDeleteApiRequest : IBaseApiRequest
    {
        [JsonIgnore]
        string DeleteUrl { get; }
    }
}