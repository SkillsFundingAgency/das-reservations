using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    public class WhenCallingPostCompleted
    {
        private IFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        }

        [Test, AutoData]
        public void And_Has_Ukprn_And_ValidationError_Then_Return_Provider_Completed_View(
            ConfirmationRedirectViewModel model, 
            ReservationsRouteModel routeModel)
        {
            var controller = _fixture.Create<ReservationsController>();
            controller.ModelState.AddModelError("AddApprentice", "AddApprentice");

            var actual = controller.PostCompleted(routeModel, model);

            var actualModel = actual as ViewResult;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual(ViewNames.ProviderCompleted, actualModel.ViewName);
        }

        [Test, AutoData]
        public void And_No_Ukprn_And_ValidationError_Then_Return_Employer_Completed_View(
            ConfirmationRedirectViewModel model, 
            ReservationsRouteModel routeModel)
        {
            routeModel.UkPrn = null;
            var controller = _fixture.Create<ReservationsController>();
            controller.ModelState.AddModelError("AddApprentice", "AddApprentice");

            var actual = controller.PostCompleted(routeModel, model);

            var actualModel = actual as ViewResult;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual(ViewNames.EmployerCompleted, actualModel.ViewName);
        }
        
        [TestCase(ConfirmationRedirectViewModel.RedirectOptions.RecruitAnApprentice)]
        [TestCase(ConfirmationRedirectViewModel.RedirectOptions.AddAnApprentice)]
        [TestCase(ConfirmationRedirectViewModel.RedirectOptions.Homepage)]
        [TestCase(ConfirmationRedirectViewModel.RedirectOptions.FindApprenticeshipTraining)]
        public void Then_The_Request_Is_Redirected_Based_On_The_Selection(string selection)
        {
            var model = _fixture.Create<ConfirmationRedirectViewModel>();
            var routeModel = _fixture.Create<ReservationsRouteModel>();
            var controller = _fixture.Create<ReservationsController>();
            model.WhatsNext = selection;

            var actual = controller.PostCompleted(routeModel, model);

            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            switch (selection)
            {
                case (ConfirmationRedirectViewModel.RedirectOptions.RecruitAnApprentice):
                    Assert.AreEqual(model.RecruitApprenticeUrl,result.Url);
                    break;

                case (ConfirmationRedirectViewModel.RedirectOptions.FindApprenticeshipTraining):
                    Assert.AreEqual(model.FindApprenticeshipTrainingUrl,result.Url);
                    break;

                case (ConfirmationRedirectViewModel.RedirectOptions.AddAnApprentice):
                    Assert.AreEqual(model.ApprenticeUrl, result.Url);
                    break;

                case (ConfirmationRedirectViewModel.RedirectOptions.Homepage):
                    Assert.AreEqual(model.DashboardUrl,result.Url);
                    break;

                default: 
                    Assert.Fail();
                    break;

            }
        }
    }
}
