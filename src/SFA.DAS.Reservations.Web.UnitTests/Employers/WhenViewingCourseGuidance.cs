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
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingCourseGuidance
    {
        [Test, MoqAutoData]
        public void Then_The_Model_Is_Populated_With_The_Correct_Values(
            string dashboardUrl,
            string findApprenticeshipTrainingUrl,
            string providerPermissionsUrl,
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [Frozen] IOptions<ReservationsWebConfiguration> configuration,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            //Arrange
            externalUrlHelper.Setup(x => x.GenerateDashboardUrl(routeModel.EmployerAccountId)).Returns(dashboardUrl);
            externalUrlHelper.Setup(x => x.GenerateUrl(
                It.Is<UrlParameters>(c => c.SubDomain.Equals("permissions") &&
                                          c.Folder.Equals("accounts") &&
                                          c.Id.Equals(routeModel.EmployerAccountId) &&
                                          c.Controller.Equals("providers"))))
                .Returns(providerPermissionsUrl);
            configuration.Value.FindApprenticeshipTrainingUrl = findApprenticeshipTrainingUrl;

            //Act
            var actual = controller.CourseGuidance(routeModel);

            //Assert
            Assert.IsNotNull(actual);
            var actualViewResult = actual as ViewResult;
            Assert.IsNotNull(actualViewResult);
            var actualModel = actualViewResult.Model as CourseGuidanceViewModel;
            Assert.IsNotNull(actualModel);
            Assert.AreEqual(dashboardUrl, actualModel.DashboardUrl);
            Assert.AreEqual(RouteNames.EmployerSelectCourse, actualModel.BackRouteName);
            Assert.AreEqual(findApprenticeshipTrainingUrl, actualModel.FindApprenticeshipTrainingUrl);
            Assert.AreEqual(providerPermissionsUrl, actualModel.ProviderPermissionsUrl);
        }
    }
}
