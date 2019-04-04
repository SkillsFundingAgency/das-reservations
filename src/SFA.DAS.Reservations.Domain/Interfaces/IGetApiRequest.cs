using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IGetApiRequest : IBaseApiRequest
    {
        [JsonIgnore]
        string GetUrl { get; }
    }
}
