using System;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReservationViewModel : AddApprenticeViewModel
    {
        public ReservationViewModel(Reservation reservation, string apprenticeUrl, uint? loggedInProviderId) : base(apprenticeUrl)
        {
            Id = reservation.Id;
            TrainingDate = new TrainingDateModel
            {
                StartDate = reservation.StartDate, 
                EndDate = reservation.ExpiryDate
            };
            
            Status = (ReservationStatusViewModel)reservation.Status;
            IsExpired = reservation.IsExpired;
            CourseName = reservation.Course != null ? reservation.Course.CourseDescription : "Unknown";
            TrainingType = ConvertToDesc(reservation.LearningType);
            LegalEntityName = reservation.AccountLegalEntityName;
            CanProviderDeleteReservation = !loggedInProviderId.HasValue || loggedInProviderId == reservation.ProviderId;
            DeleteRouteName = (ReservationStatusViewModel) reservation.Status == ReservationStatusViewModel.Pending && !reservation.IsExpired
                ? (loggedInProviderId == null ? RouteNames.EmployerDelete : RouteNames.ProviderDelete)
                : string.Empty;
        }

        private string ConvertToDesc(LearningType? learningType)
        {
            return learningType switch
            {
                LearningType.Apprenticeship => "Apprenticeship",
                LearningType.FoundationApprenticeship => "Foundation apprenticeship",
                LearningType.ApprenticeshipUnit => "Apprenticeship unit",
                _ => "Apprenticeship"
            };
        }

        public Guid Id { get; }
        public ReservationStatusViewModel Status { get; }
        public bool IsExpired { get; set; }
        public string LegalEntityName { get; }
        public string CourseName { get; }
        public string TrainingType { get; }
        public TrainingDateModel TrainingDate { get; }
        public bool CanProviderDeleteReservation { get; }
        public string DeleteRouteName { get; }
    }
}
