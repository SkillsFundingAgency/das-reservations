using System;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ConfirmationRedirectViewModel
    {
        [Required(ErrorMessage = "Select what you would like to do next")]
        public string WhatsNext { get; set; }
        public string DashboardUrl { get; set; }
        public string ApprenticeUrl { get; set; }
        public string RecruitApprenticeUrl { get; set; }
        public string FindTrainingProviderUrl { get; set; }
        public Guid ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string CourseId { get; set; }
        public int Level { get; set; }
        public string CourseTitle { get; set; }

        public struct RedirectOptions
        {
            public const string RecruitAnApprentice = "recruit-an-apprentice";
            public const string AddAnApprentice = "add-an-apprentice";
            public const string ProviderHomepage = "provider-homepage";
            public const string FindTrainingProvider = "find-training-provider";
        }

    }
}