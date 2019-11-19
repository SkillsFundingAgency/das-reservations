using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface ISearchApiRequest : IBaseApiRequest
    {
        [JsonIgnore]
        string SearchUrl { get; }
    }
}