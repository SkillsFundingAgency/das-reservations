using Microsoft.WindowsAzure.Storage.Table;

namespace SFA.DAS.Reservations.Models.Configuration
{
    public class ConfigurationItem : TableEntity
    {
        public string Data { get; set; }
    }
}