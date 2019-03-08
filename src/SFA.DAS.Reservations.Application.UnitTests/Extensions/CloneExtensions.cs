using Newtonsoft.Json;

namespace SFA.DAS.Reservations.Application.UnitTests.Extensions
{
    public static class CloneExtensions
    {
        public static T Clone<T>(this T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}