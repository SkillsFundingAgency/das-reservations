using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IGetAllApiRequest : IBaseApiRequest
    {
        [JsonIgnore]
        string GetAllUrl { get; }
    }
}
