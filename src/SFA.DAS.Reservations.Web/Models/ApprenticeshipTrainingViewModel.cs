using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ApprenticeshipTrainingViewModel : SelectCourseViewModel
    {
        public IEnumerable<TrainingDateViewModel> PossibleStartDates { get; set; }
        public string RouteName { get; set; }
        public bool IsProvider { get; set; }
        public string BackLink { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string CohortRef { get; set; }
        public bool? FromReview { get; set; }
        public GlobalRule ActiveGlobalRule { get; set; }
        public string ActiveRuleActiveToDateText => ActiveGlobalRule.ActiveTo?.ToString("MMMM yyyy");
    }
}