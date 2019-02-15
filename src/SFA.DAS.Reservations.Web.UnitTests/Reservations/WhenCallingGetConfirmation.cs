using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetConfirmation
    {
        [Test, MoqAutoData]
        public void Then_It_Returns_The_ViewModel(
            ReservationsRouteModel routeModel,
            ReservationsController controller)
        {
            var result = controller.Confirmation(routeModel);
            result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ReservationsRouteModel>()
                .Which.Should().BeSameAs(routeModel);
        }
    }
}