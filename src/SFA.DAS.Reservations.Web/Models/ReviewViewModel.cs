﻿using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ReviewViewModel
    {
        public ReviewViewModel(ReservationsRouteModel routeModel,string startDateDescription, string courseDescription, string accountLegalEntityName, string accountLegalEntityPublicHashedId)
        {
            ConfirmRouteName = IsEmployerRoute(routeModel) ?
                RouteNames.EmployerCreateReservation :
                RouteNames.ProviderCreateReservation;

            ChangeRouteName = IsEmployerRoute(routeModel) ?
                RouteNames.EmployerApprenticeshipTraining :
                RouteNames.ProviderApprenticeshipTraining;

            ViewName = IsEmployerRoute(routeModel) ?
                ViewNames.EmployerReview :
                ViewNames.ProviderReview;

            RouteModel = routeModel;
            StartDateDescription = startDateDescription;
            CourseDescription = courseDescription;
            AccountLegalEntityName = accountLegalEntityName;
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;

            
        }

        public string ConfirmRouteName { get; }
        public string ChangeRouteName { get; }
        public ReservationsRouteModel RouteModel { get;  }
        public string StartDateDescription { get;  }
        public string CourseDescription { get;  }
        public string AccountLegalEntityName { get;  }
        public string AccountLegalEntityPublicHashedId { get;  }
        public string ViewName { get; }

        private bool IsEmployerRoute(ReservationsRouteModel routeModel) => routeModel.UkPrn == null;
        
    }
}