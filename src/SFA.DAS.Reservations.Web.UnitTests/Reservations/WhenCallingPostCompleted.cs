using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
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
            CompletedViewModel model, 
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
            CompletedViewModel model, 
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
        
        [TestCase(CompletedReservationWhatsNext.RecruitAnApprentice)]
        [TestCase(CompletedReservationWhatsNext.AddAnApprentice)]
        [TestCase(CompletedReservationWhatsNext.Homepage)]
        [TestCase(CompletedReservationWhatsNext.FindApprenticeshipTraining)]
        public void And_Has_Ukprn_Then_The_Request_Is_Redirected_Based_On_The_Selection(CompletedReservationWhatsNext selection)
        {
            var model = _fixture.Create<CompletedViewModel>();
            model.WhatsNext = selection;
            var routeModel = _fixture.Create<ReservationsRouteModel>();

            var config = _fixture.Freeze<IOptions<ReservationsWebConfiguration>>();
            var providerRecruitUrl = _fixture.Create<string>();
            var addApprenticeUrl = _fixture.Create<string>();
            var mockUrlHelper = _fixture.Freeze<Mock<IExternalUrlHelper>>();
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(routeModel.UkPrn.ToString(), "", "", "recruit", "", ""))
                .Returns(providerRecruitUrl);
            mockUrlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(model.UkPrn, routeModel.Id.Value, routeModel.AccountLegalEntityPublicHashedId, model.StartDate, model.CourseId))
                .Returns(addApprenticeUrl);
            var controller = _fixture.Create<ReservationsController>();
            
            var actual = controller.PostCompleted(routeModel, model);

            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            switch (selection)
            {
                case (CompletedReservationWhatsNext.RecruitAnApprentice):
                    Assert.AreEqual(providerRecruitUrl,result.Url);
                    break;

                case (CompletedReservationWhatsNext.FindApprenticeshipTraining):
                    Assert.AreEqual(config.Value.FindApprenticeshipTrainingUrl,result.Url);
                    break;

                case (CompletedReservationWhatsNext.AddAnApprentice):
                    Assert.AreEqual(addApprenticeUrl, result.Url);
                    break;

                case (CompletedReservationWhatsNext.Homepage):
                    Assert.AreEqual(config.Value.DashboardUrl,result.Url);
                    break;

                default: 
                    Assert.Fail();
                    break;
            }
        }

        [TestCase(CompletedReservationWhatsNext.RecruitAnApprentice)]
        [TestCase(CompletedReservationWhatsNext.AddAnApprentice)]
        [TestCase(CompletedReservationWhatsNext.Homepage)]
        [TestCase(CompletedReservationWhatsNext.FindApprenticeshipTraining)]
        public void And_No_Ukprn_Then_The_Request_Is_Redirected_Based_On_The_Selection(CompletedReservationWhatsNext selection)
        {
            var model = _fixture.Create<CompletedViewModel>();
            model.WhatsNext = selection;
            var routeModel = _fixture.Create<ReservationsRouteModel>();
            routeModel.UkPrn = null;

            var config = _fixture.Freeze<IOptions<ReservationsWebConfiguration>>();
            var employerRecruitUrl = _fixture.Create<string>();
            var addApprenticeUrl = _fixture.Create<string>();
            var mockUrlHelper = _fixture.Freeze<Mock<IExternalUrlHelper>>();
            mockUrlHelper
                .Setup(helper =>
                    helper.GenerateUrl(routeModel.EmployerAccountId, "", "", "recruit", "accounts", ""))
                .Returns(employerRecruitUrl);
            mockUrlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(model.UkPrn, routeModel.Id.Value, routeModel.AccountLegalEntityPublicHashedId, model.StartDate, model.CourseId))
                .Returns(addApprenticeUrl);
            var controller = _fixture.Create<ReservationsController>();
            
            var actual = controller.PostCompleted(routeModel, model);

            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            switch (selection)
            {
                case (CompletedReservationWhatsNext.RecruitAnApprentice):
                    Assert.AreEqual(employerRecruitUrl,result.Url);
                    break;

                case (CompletedReservationWhatsNext.FindApprenticeshipTraining):
                    Assert.AreEqual(config.Value.FindApprenticeshipTrainingUrl,result.Url);
                    break;

                case (CompletedReservationWhatsNext.AddAnApprentice):
                    Assert.AreEqual(addApprenticeUrl, result.Url);
                    break;

                case (CompletedReservationWhatsNext.Homepage):
                    Assert.AreEqual(config.Value.EmployerDashboardUrl,result.Url);
                    break;

                default: 
                    Assert.Fail();
                    break;
            }
        }
    }
}
