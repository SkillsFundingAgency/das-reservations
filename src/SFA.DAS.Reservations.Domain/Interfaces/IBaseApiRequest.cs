using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IBaseApiRequest
    {
        [JsonIgnore]
        string BaseUrl { get; }
    }
}