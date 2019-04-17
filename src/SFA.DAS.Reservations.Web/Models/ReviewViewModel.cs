using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReviewViewModel
    {
        public ReviewViewModel(ReservationsRouteModel routeModel,string startDateDescription, string courseDescription, string accountLegalEntityName, string accountLegalEntityPublicHashedId)
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
                 string.Empty : RouteNames.ProviderApprenticeshipTraining;

            RouteModel = routeModel;
            StartDateDescription = startDateDescription;
            CourseDescription = courseDescription;
            AccountLegalEntityName = accountLegalEntityName;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
        }

        public string ChangeCourseRouteName { get; }
        public string ChangeStartDateRouteName { get; }
        public string ConfirmRouteName { get; }
        public ReservationsRouteModel RouteModel { get;  }
        public string StartDateDescription { get;  }
        public string CourseDescription { get;  }
        public string AccountLegalEntityName { get;  }
        public string AccountLegalEntityPublicHashedId { get;  }
        public string ViewName { get; }
        public string BackLink { get;}

        private bool IsEmployerRoute(ReservationsRouteModel routeModel) => routeModel.UkPrn == null;
        
    }
}