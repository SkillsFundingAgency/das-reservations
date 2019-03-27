using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    public class WhenCallingPostConfirmation
    {
        private IFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        }

        [Test, AutoData]
        public async Task Then_The_Model_Is_Validated_And_Confirmation_Returned(ConfirmationRedirectViewModel model, ReservationsRouteModel routeModel)
        {
            var controller = _fixture.Create<ReservationsController>();
            controller.ModelState.AddModelError("AddApprentice", "AddApprentice");

            var actual = await controller.Completed(routeModel, model);

            var actualModel = actual as ViewResult;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual("Confirmation",actualModel.ViewName);
        }

        [TestCase(true)]
        [TestCase(false)]
        [AutoData]
        public async Task Then_The_Request_Is_Redirected_Based_On_The_Selection(bool selection, ConfirmationRedirectViewModel model, ReservationsRouteModel routeModel)
        {
            model.AddApprentice = selection;
            var controller = _fixture.Create<ReservationsController>();

            var actual = await controller.Completed(routeModel, model);

            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(selection ? model.ApprenticeUrl : model.DashboardUrl, result.Url);
        }
    }
}
