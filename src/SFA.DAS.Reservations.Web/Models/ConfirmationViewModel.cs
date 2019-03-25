using System;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmationViewModel
    {
        public ConfirmationViewModel(Guid reservationId,
            DateTime startDate, DateTime expiryDate, Course course, uint? providerId = null, string dashboardUrl = "", string apprenticeUrl = "")
        {
            ReservationId = reservationId;
            StartDate = startDate;
            ExpiryDate = expiryDate;
            Course = course;
            ProviderId = providerId;
            if (!string.IsNullOrEmpty(apprenticeUrl))
            {
                if (course != null)
                {
                    apprenticeUrl = $"{apprenticeUrl}/{providerId}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId=YZWX27&courseCode={course.Id}&startMonthYear={StartDate.Month}{StartDate.Year}";
                }
                else
                {
                    apprenticeUrl = $"{apprenticeUrl}/{providerId}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId=YZWX27&startMonthYear={StartDate.Month}{StartDate.Year}";
                }
            }
            

            DashboardUrl = dashboardUrl;
            ApprenticeUrl = apprenticeUrl;
        }

        public Guid ReservationId { get; }
        public DateTime StartDate { get; }
        public DateTime ExpiryDate { get; }
        public Course Course { get; }
        public uint? ProviderId { get; }
        public string DashboardUrl { get;}
		public string ApprenticeUrl { get; }
        public bool ShowDashboardUrl => !string.IsNullOrEmpty(DashboardUrl);
        public bool ShowApprenticeUrl => !string.IsNullOrEmpty(ApprenticeUrl);
		public string AccountLegalEntityName { get; }
    }
}