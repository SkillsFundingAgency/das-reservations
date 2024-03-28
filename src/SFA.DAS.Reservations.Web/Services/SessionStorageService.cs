using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Web.Services
{
    public class SessionStorageService<T> : ISessionStorageService<T>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionStorageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Store(T testModel)
        {
            _httpContextAccessor.HttpContext.Session.SetString(typeof(T).Name, JsonConvert.SerializeObject(testModel));
        }

        public T Get()
        {
            var valueStored = _httpContextAccessor.HttpContext.Session.GetString(typeof(T).Name);
            
            if (!string.IsNullOrEmpty(valueStored))
            {
                return JsonConvert.DeserializeObject<T>(valueStored);
            }

            return default;
        }

        public void Delete()
        {
            _httpContextAccessor.HttpContext.Session.Remove(typeof(T).Name);
        }
    }
}
