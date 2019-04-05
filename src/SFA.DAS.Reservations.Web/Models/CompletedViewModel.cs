using System;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Models
{
    public class CompletedViewModel
    {
        public CompletedViewModel(Guid reservationId, DateTime startDate, DateTime expiryDate, Course course, string accountLegalEntityPublicHashedId, uint? ukPrn = null,string accountLegalEntityName = "", string dashboardUrl = "", string apprenticeUrl = "")
        {
            ReservationId = reservationId;
            StartDate = startDate;
            ExpiryDate = expiryDate;
            Course = course;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
            UkPrn = ukPrn;
            AccountLegalEntityName = accountLegalEntityName;

            if (!string.IsNullOrWhiteSpace(apprenticeUrl))
            {
                apprenticeUrl = $"{apprenticeUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={StartDate:MMyyyy}";
                if (course != null)
                {
                    apprenticeUrl += $"&courseCode={course.Id}";
                }
            }
            
            DashboardUrl = dashboardUrl;
            ApprenticeUrl = apprenticeUrl;

            ViewName = ukPrn == null ? ViewNames.EmployerCompleted : ViewNames.ProviderCompleted;
        }

        public Guid ReservationId { get; }
        public DateTime StartDate { get; }
        public DateTime ExpiryDate { get; }
        public Course Course { get; }
        public string AccountLegalEntityPublicHashedId { get; }
        public uint? UkPrn { get; }
        public string DashboardUrl { get;}
		public string ApprenticeUrl { get; }
        public bool ShowDashboardUrl => !string.IsNullOrWhiteSpace(DashboardUrl);
        public bool ShowApprenticeUrl => !string.IsNullOrWhiteSpace(ApprenticeUrl);
		public string AccountLegalEntityName { get; }
        public string ViewName { get; }
    }
}