using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmationViewModel
    {
        public ConfirmationViewModel(Guid reservationId, DateTime startDate, DateTime expiryDate, Course course, string accountLegalEntityPublicHashedId, uint? providerId = null,string accountLegalEntityName = "", string dashboardUrl = "", string apprenticeUrl = "")
        {
            ReservationId = reservationId;
            StartDate = startDate;
            ExpiryDate = expiryDate;
            Course = course;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
            ProviderId = providerId;
            AccountLegalEntityName = accountLegalEntityName;

            if (!string.IsNullOrWhiteSpace(apprenticeUrl))
            {
                apprenticeUrl = $"{apprenticeUrl}/{providerId}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={StartDate.ToString("MMyyyy")}";
                if (course != null)
                {
                    apprenticeUrl += $"&courseCode={course.Id}";
                }
            }
            
            DashboardUrl = dashboardUrl;
            ApprenticeUrl = apprenticeUrl;
        }

        public Guid ReservationId { get; }
        public DateTime StartDate { get; }
        public DateTime ExpiryDate { get; }
        public Course Course { get; }
        public string AccountLegalEntityPublicHashedId { get; }
        public uint? ProviderId { get; }
        public string DashboardUrl { get;}
		public string ApprenticeUrl { get; }
        public bool ShowDashboardUrl => !string.IsNullOrWhiteSpace(DashboardUrl);
        public bool ShowApprenticeUrl => !string.IsNullOrWhiteSpace(ApprenticeUrl);
		public string AccountLegalEntityName { get; }
    }
}