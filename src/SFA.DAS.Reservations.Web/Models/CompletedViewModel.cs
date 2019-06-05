using System;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Models
{
    public class CompletedViewModel : AddApprenticeViewModel
    {
        public CompletedViewModel(
            Guid reservationId, 
            DateTime startDate, 
            DateTime expiryDate, 
            Course course, 
            string accountLegalEntityPublicHashedId, 
            uint? ukPrn = null,
            string accountLegalEntityName = "", 
            string dashboardUrl = "", 
            string apprenticeUrl = "",
            string employerDashboardUrl = "",
            string recruitApprenticeUrl = "",
            string findApprenticeshipTrainingUrl = ""): base(apprenticeUrl, ukPrn, reservationId, accountLegalEntityPublicHashedId, startDate, course)
        {
            ReservationId = reservationId;
            StartDate = startDate;
            ExpiryDate = expiryDate;
            Course = course;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
            UkPrn = ukPrn;
            AccountLegalEntityName = accountLegalEntityName;
            DashboardUrl = dashboardUrl;
            RecruitApprenticeUrl = recruitApprenticeUrl;
            EmployerDashboardUrl = employerDashboardUrl;
            FindApprenticeshipTrainingUrl = findApprenticeshipTrainingUrl;


            ViewName = ukPrn == null ? ViewNames.EmployerCompleted : ViewNames.ProviderCompleted;
        }

        public Guid ReservationId { get; }
        public DateTime StartDate { get; }
        public DateTime ExpiryDate { get; }
        public Course Course { get; }
        public string AccountLegalEntityPublicHashedId { get; }
        public uint? UkPrn { get; }
        public string DashboardUrl { get;}
        public string RecruitApprenticeUrl { get; }
        public bool ShowDashboardUrl => !string.IsNullOrWhiteSpace(DashboardUrl);
        public bool ShowApprenticeUrl => !string.IsNullOrWhiteSpace(ApprenticeUrl);
		public string AccountLegalEntityName { get; }
        public string ViewName { get; }
        public string EmployerDashboardUrl { get; }
        public string FindApprenticeshipTrainingUrl { get; }
    }
}