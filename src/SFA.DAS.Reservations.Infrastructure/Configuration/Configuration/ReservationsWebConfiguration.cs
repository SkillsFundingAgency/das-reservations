namespace SFA.DAS.Reservations.Infrastructure.Configuration.Configuration
{
    public class ReservationsWebConfiguration
    {
        public string HashSalt { get; set; }
        public int HashLength { get; set; }
        public string HashAlphabet { get; set; }
        public double SessionTimeoutHours { get; set; }
    }
}