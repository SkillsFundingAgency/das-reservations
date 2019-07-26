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
            string accountLegalEntityPublicHashedId,
            DateTime startDate, 
            Course course)
        {
            if (!string.IsNullOrWhiteSpace(apprenticeUrl))
            {
                apprenticeUrl = $"{apprenticeUrl}/{ukPrn}/unapproved/add-apprentice?reservationId={reservationId}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={startDate:MMyyyy}";
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