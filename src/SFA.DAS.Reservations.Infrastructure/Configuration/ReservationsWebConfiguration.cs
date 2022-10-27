using System;

namespace SFA.DAS.Reservations.Infrastructure.Configuration
{
    public class ReservationsWebConfiguration
    {
        public double SessionTimeoutHours { get; set; }
        
        public string RedisCacheConnectionString { get; set; }
        public string DataProtectionKeysDatabase { get; set; }
        public virtual string ApprenticeUrl { get; set; }
        public virtual string EmployerApprenticeUrl { get; set; }
        public virtual string DashboardUrl { get; set; }
        public virtual string EmployerDashboardUrl { get; set; }
        public virtual string FindApprenticeshipTrainingUrl { get; set; }
        public virtual string ApprenticeshipFundingRulesUrl { get; set; }
        public bool UseGovSignIn { get; set; }  
    }
}