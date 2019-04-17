using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingTheStartPage
    {
        [Test, MoqAutoData]
        public void Then_Returns_The_Index_View(
            EmployerReservationsController controller)
        {
            var result = controller.Index() as ViewResult;

            result.Should().NotBeNull();
        }
    }
}
