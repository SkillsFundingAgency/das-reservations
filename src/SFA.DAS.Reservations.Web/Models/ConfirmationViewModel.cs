using System;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmationViewModel
    {
        public ConfirmationViewModel(Guid reservationId, DateTime startDate, DateTime expiryDate, Course course, string hashedAccountLegalEntityId, uint? providerId = null,string accountLegalEntityName = "", string dashboardUrl = "", string apprenticeUrl = "")
        {
            ReservationId = reservationId;
            StartDate = startDate;
            ExpiryDate = expiryDate;
            Course = course;
            HashedAccountLegalEntityId = hashedAccountLegalEntityId;
            ProviderId = providerId;
            AccountLegalEntityName = accountLegalEntityName;

            if (!string.IsNullOrEmpty(apprenticeUrl))
            {
                apprenticeUrl = $"{apprenticeUrl}/{providerId}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={hashedAccountLegalEntityId}&startMonthYear={StartDate.Month}{StartDate.Year}";
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
        public string HashedAccountLegalEntityId { get; }
        public uint? ProviderId { get; }
        public string DashboardUrl { get;}
		public string ApprenticeUrl { get; }
        public bool ShowDashboardUrl => !string.IsNullOrEmpty(DashboardUrl);
        public bool ShowApprenticeUrl => !string.IsNullOrEmpty(ApprenticeUrl);
		public string AccountLegalEntityName { get; }
    }
}