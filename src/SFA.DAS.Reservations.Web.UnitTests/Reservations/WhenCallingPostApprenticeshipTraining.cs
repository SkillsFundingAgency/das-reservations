using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostApprenticeshipTraining
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Redirects_To_Provider_Route(
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            var result = await controller.PostApprenticeshipTraining(routeModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("provider-review");
        }

        [Test, MoqAutoData]
        public async Task And_No_Ukprn_Then_Redirects_To_Employer_Route(
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            routeModel.Ukprn = null;
            var result = await controller.PostApprenticeshipTraining(routeModel) as RedirectToRouteResult;
            
            result.Should().NotBeNull($"result was not a {typeof(RedirectToRouteResult)}");
            result.RouteName.Should().Be("employer-review");
        }
    }
}