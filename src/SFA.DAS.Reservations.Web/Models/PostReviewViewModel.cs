using System.ComponentModel.DataAnnotations;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Models
{
    public class PostReviewViewModel
    {
        public TrainingDateModel TrainingDate { get; set; }
        public string CourseDescription { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }        
    }
}