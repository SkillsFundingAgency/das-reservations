namespace SFA.DAS.Reservations.Infrastructure.Configuration
{
    public class ProviderIdamsConfiguration
    {
        public string MetadataAddress { get; set; }

        public string Wtrealm  {get; set; }
        public bool UseStub { get; set; }
    }
}