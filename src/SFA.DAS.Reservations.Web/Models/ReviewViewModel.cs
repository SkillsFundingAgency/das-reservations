using System;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReviewViewModel
    {
        public ReviewViewModel(ReservationsRouteModel routeModel, TrainingDateModel trainingDate, string courseDescription, string accountLegalEntityName, string accountLegalEntityPublicHashedId)
        {
            ConfirmRouteName = IsEmployerRoute(routeModel) ?
                RouteNames.EmployerPostReview :
                RouteNames.ProviderPostReview;

            ChangeCourseRouteName = IsEmployerRoute(routeModel) ?
                RouteNames.EmployerSelectCourse :
                RouteNames.ProviderApprenticeshipTraining;

            ChangeStartDateRouteName = IsEmployerRoute(routeModel) ?
                RouteNames.EmployerApprenticeshipTraining :
                RouteNames.ProviderApprenticeshipTraining;

            ViewName = IsEmployerRoute(routeModel) ?
                ViewNames.EmployerReview :
                ViewNames.ProviderReview;

            BackLink = IsEmployerRoute(routeModel) ? 
                RouteNames.EmployerApprenticeshipTraining : RouteNames.ProviderApprenticeshipTraining;

            RouteModel = routeModel;
            TrainingDate = trainingDate;
            CourseDescription = courseDescription;
            AccountLegalEntityName = accountLegalEntityName;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
        }

        public ReviewViewModel(ReservationsRouteModel routeModel, PostReviewViewModel postReviewViewModel) : 
            this(routeModel, 
                new TrainingDateModel{StartDate = postReviewViewModel.StartDate, EndDate = postReviewViewModel.EndDate}, 
                postReviewViewModel.CourseDescription,
                postReviewViewModel.AccountLegalEntityName,
                postReviewViewModel.AccountLegalEntityPublicHashedId)
        {
            Reserve = postReviewViewModel.Reserve;
        }

        public string ChangeCourseRouteName { get; }
        public string ChangeStartDateRouteName { get; }
        public string ConfirmRouteName { get; }
        public ReservationsRouteModel RouteModel { get;  }
        public TrainingDateModel TrainingDate { get;  }
        public string CourseDescription { get;  }
        public string AccountLegalEntityName { get;  }
        public string AccountLegalEntityPublicHashedId { get;  }
        public string ViewName { get; }
        public string BackLink { get;}
        public bool? Reserve { get; }

        private bool IsEmployerRoute(ReservationsRouteModel routeModel) => routeModel.UkPrn == null;
        
    }
}