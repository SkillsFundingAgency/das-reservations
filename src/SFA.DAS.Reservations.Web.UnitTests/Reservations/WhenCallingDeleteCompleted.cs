using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingDeleteCompleted
    {
        [Test, MoqAutoData]
        public void And_Has_Ukprn_Then_Returns_Provider_Delete_Completed_View(
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            var result = controller.DeleteCompleted(routeModel) as ViewResult;

            result.Should().NotBeNull();
            result.ViewName.Should().Be(ViewNames.ProviderDeleteCompleted);
        }

        [Test, MoqAutoData]
        public void And_No_Ukprn_Then_Returns_Employer_Delete_Completed_View(
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            routeModel.UkPrn = null;

            var result = controller.DeleteCompleted(routeModel) as ViewResult;

            result.Should().NotBeNull();
            result.ViewName.Should().Be(ViewNames.EmployerDeleteCompleted);
        }
    }
}