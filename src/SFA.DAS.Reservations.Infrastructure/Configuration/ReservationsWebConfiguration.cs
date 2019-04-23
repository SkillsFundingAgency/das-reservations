using System;

namespace SFA.DAS.Reservations.Infrastructure.Configuration
{
    public class ReservationsWebConfiguration
    {
        public string EmployerAccountHashSalt { get; set; }
        public int EmployerAccountHashLength { get; set; }
        public string EmployerAccountHashAlphabet { get; set; }
        public double SessionTimeoutHours { get; set; }
        public DateTime? CurrentDateTime { get; set; }

        public string RedisCacheConnectionString
        {
            get; set;

        }
        public virtual string ApprenticeUrl { get; set; }
        public virtual string DashboardUrl { get; set; }
        public virtual string EmployerDashboardUrl { get; set; }
    }
}