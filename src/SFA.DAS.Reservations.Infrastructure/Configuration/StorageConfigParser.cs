using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SFA.DAS.Reservations.Models.Configuration;

namespace SFA.DAS.Reservations.Infrastructure.Configuration
{
    public class StorageConfigParser
    {
        public Dictionary<string, string> ParseConfig(ConfigurationItem configItem)
        {
            var configDictionary = new Dictionary<string,string>();

            var jsonObject = JObject.Parse(configItem.Data);

            foreach (var child in jsonObject.Children())
            {
                foreach (var jToken in child.Children().Children())
                {
                    var child1 = (JProperty)jToken;
                    configDictionary.Add($"{child.Path}:{child1.Name}", child1.Value.ToString());
                }
            }
            return configDictionary;
        }
    }
}
