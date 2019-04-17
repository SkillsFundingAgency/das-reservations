using Microsoft.WindowsAzure.Storage.Table;

namespace SFA.DAS.Reservations.Infrastructure.Configuration
{
    public class ConfigurationItem : TableEntity
    {
        public string Data { get; set; }
    }
}