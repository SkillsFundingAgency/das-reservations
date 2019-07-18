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
            model.CohortRef = string.Empty;
            var config = _fixture.Freeze<IOptions<ReservationsWebConfiguration>>();
            var providerRecruitUrl = _fixture.Create<string>();
            var addApprenticeUrl = _fixture.Create<string>();
            var homeUrl = _fixture.Create<string>();
            var mockUrlHelper = _fixture.Freeze<Mock<IExternalUrlHelper>>();
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Id == routeModel.UkPrn.ToString() && 
                        parameters.SubDomain == "recruit")))
                .Returns(providerRecruitUrl);
            mockUrlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Id == model.UkPrn.ToString() &&
                        parameters.Controller == "unapproved" &&
                        parameters.Action == "add-apprentice" &&
                        parameters.QueryString == $"?reservationId={routeModel.Id.Value}&employerAccountLegalEntityPublicHashedId={routeModel.AccountLegalEntityPublicHashedId}&startMonthYear={model.StartDate:MMyyyy}&courseCode={model.CourseId}")))
                .Returns(addApprenticeUrl);
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters => parameters.Controller == "account")))
                .Returns(homeUrl);
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
                    Assert.AreEqual(homeUrl,result.Url);
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
            model.CohortRef = string.Empty;
            model.UkPrn = null;
            var config = _fixture.Freeze<IOptions<ReservationsWebConfiguration>>();
            var employerRecruitUrl = _fixture.Create<string>();
            var addApprenticeUrl = _fixture.Create<string>();
            var homeUrl = _fixture.Create<string>();
            var mockUrlHelper = _fixture.Freeze<Mock<IExternalUrlHelper>>();
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Id == routeModel.EmployerAccountId &&
                        parameters.SubDomain == "recruit" &&
                        parameters.Folder == "accounts")))
                .Returns(employerRecruitUrl);
            mockUrlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Id == routeModel.EmployerAccountId &&
                        parameters.Controller == "unapproved" &&
                        parameters.Action == "add" &&
                        parameters.QueryString == $"?reservationId={routeModel.Id.Value}&employerAccountLegalEntityPublicHashedId={routeModel.AccountLegalEntityPublicHashedId}&startMonthYear={model.StartDate:MMyyyy}&courseCode={model.CourseId}")))
                .Returns(addApprenticeUrl);
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Id == routeModel.EmployerAccountId &&
                        parameters.Controller == "teams" &&
                        parameters.Folder == "accounts")))
                .Returns(homeUrl);
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
                    Assert.AreEqual(homeUrl,result.Url);
                    break;

                default: 
                    Assert.Fail();
                    break;
            }
        }

        [Test]
        public void Then_When_There_Is_A_Cohort_Ref_The_Add_Apprentice_Link_Includes_The_Reference()
        {
            //Arrange
            var model = _fixture.Create<CompletedViewModel>();
            model.WhatsNext = CompletedReservationWhatsNext.AddAnApprentice;
            var routeModel = _fixture.Create<ReservationsRouteModel>();
            var addApprenticeUrl = _fixture.Create<string>();
            var mockUrlHelper = _fixture.Freeze<Mock<IExternalUrlHelper>>();
            mockUrlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Id == model.UkPrn.ToString() &&
                        parameters.Controller == $"unapproved/{model.CohortRef}" &&
                        parameters.Action == "apprentices/add" &&
                        parameters.QueryString == $"?reservationId={routeModel.Id.Value}" +
                        $"&employerAccountLegalEntityPublicHashedId={routeModel.AccountLegalEntityPublicHashedId}" +
                        $"&startMonthYear={model.StartDate:MMyyyy}" +
                        $"&courseCode={model.CourseId}")))
                .Returns(addApprenticeUrl);
            var controller = _fixture.Create<ReservationsController>();

            //Act
            var actual = controller.PostCompleted(routeModel, model);
            
            //Assert
            var result = actual as RedirectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(addApprenticeUrl, result.Url);
        }
    }
}
