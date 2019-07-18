using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Web.Models
{
    public abstract class AddApprenticeViewModel
    {
        public AddApprenticeViewModel(
            string apprenticeUrl, 
            uint? ukPrn, 
            Guid reservationId, 
            string accountHashedId,
            string accountLegalEntityPublicHashedId,
            DateTime startDate, 
            Course course)
        {
            if (!string.IsNullOrWhiteSpace(apprenticeUrl))
            {
                if (ukPrn.HasValue)
                    apprenticeUrl = $"{apprenticeUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}";
                else
                    apprenticeUrl = $"{apprenticeUrl}/{accountHashedId}/unapproved/add?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}";

                if (course != null)
                {
                    apprenticeUrl += $"&courseCode={course.Id}";
                }
            }

            ApprenticeUrl = apprenticeUrl;
        }

        public string ApprenticeUrl { get; }
    }
}