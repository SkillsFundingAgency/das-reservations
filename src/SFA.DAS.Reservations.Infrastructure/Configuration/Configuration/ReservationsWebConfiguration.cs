using System;

namespace SFA.DAS.Reservations.Infrastructure.Configuration.Configuration
{
    public class ReservationsWebConfiguration
    {
        public string EmployerAccountHashSalt { get; set; }
        public int EmployerAccountHashLength { get; set; }
        public string EmployerAccountHashAlphabet { get; set; }
        public double SessionTimeoutHours { get; set; }
        public DateTime? CurrentDateTime { get; set; }
        public string RedisCacheConnectionString { get; set; }
    }
}