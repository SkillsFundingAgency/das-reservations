using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public static class RouteNames
    {
        public const string ProviderApprenticeshipTraining = "provider-apprenticeship-training";
        public const string ProviderCreateApprenticeshipTraining = "provider-create-apprenticeship-training";
        public const string ProviderReview = "provider-review";
        public const string ProviderCreateReservation = "provider-create-reservation";
        public const string ProviderReservationCreated = "provider-reservation-created";
        public const string ProviderSignOut = "provider-signout";


        public const string EmployerCreateApprenticeshipTraining = "employer-create-apprenticeship-training";
        public const string EmployerApprenticeshipTraining = "employer-apprenticeship-training";
        public const string EmployerReview = "employer-review";
        public const string EmployerCreateReservation = "employer-create-reservation";
        public const string EmployerReservationCreated = "employer-reservation-created";
        public const string EmployerSignOut = "provider-signout";
    }
}
