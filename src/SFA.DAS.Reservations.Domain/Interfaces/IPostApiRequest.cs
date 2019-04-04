using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IPostApiRequest : IBaseApiRequest
    {
        [JsonIgnore]
        string CreateUrl { get; }
    }
}