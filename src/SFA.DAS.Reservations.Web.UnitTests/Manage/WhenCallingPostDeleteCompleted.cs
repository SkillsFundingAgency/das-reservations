using AutoFixture.NUnit3;
using FluentAssertions;
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

namespace SFA.DAS.Reservations.Web.UnitTests.Manage
{
    [TestFixture]
    public class WhenCallingPostDeleteCompleted
    {
        [Test, MoqAutoData]
        public void And_Has_Ukprn_And_Model_Invalid_Then_Returns_Provider_Completed_View(
            ReservationsRouteModel routeModel,
            DeleteCompletedViewModel viewModel,
            ManageReservationsController controller)
        {
            controller.ModelState.AddModelError("key", "error message");

            var result = controller.PostDeleteCompleted(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.ProviderDeleteCompleted);
            result.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public void And_No_Ukprn_And_Model_Invalid_Then_Returns_Employer_Completed_View(
            ReservationsRouteModel routeModel,
            DeleteCompletedViewModel viewModel,
            ManageReservationsController controller)
        {
            routeModel.UkPrn = null;
            controller.ModelState.AddModelError("key", "error message");

            var result = controller.PostDeleteCompleted(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.EmployerDeleteCompleted);
            result.Model.Should().Be(viewModel);
        }

        [Test, MoqAutoData]
        public void And_Has_Ukprn_And_Manage_True_Then_Redirects_To_Provider_Manage(
            ReservationsRouteModel routeModel,
            DeleteCompletedViewModel viewModel,
            ManageReservationsController controller)
        {
            viewModel.Manage = true;

            var result = controller.PostDeleteCompleted(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderManage);
        }

        [Test, MoqAutoData]
        public void And_No_Ukprn_And_Manage_True_Then_Redirects_To_Employer_Manage(
            ReservationsRouteModel routeModel,
            DeleteCompletedViewModel viewModel,
            ManageReservationsController controller)
        {
            routeModel.UkPrn = null;
            viewModel.Manage = true;

            var result = controller.PostDeleteCompleted(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.EmployerManage);
        }

        [Test, MoqAutoData]
        public void And_Has_Ukprn_And_Manage_False_Then_Redirects_To_Provider_Dashboard(
            ReservationsRouteModel routeModel,
            DeleteCompletedViewModel viewModel,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> mockOptions,
            ManageReservationsController controller)
        {
            viewModel.Manage = false;

            var result = controller.PostDeleteCompleted(routeModel, viewModel) as RedirectResult;

            result.Url.Should().Be(mockOptions.Object.Value.DashboardUrl);
        }

        [Test, MoqAutoData]
        public void And_No_Ukprn_And_Manage_False_Then_Redirects_To_Employer_Dashboard(
            ReservationsRouteModel routeModel,
            DeleteCompletedViewModel viewModel,
            string expectedUrl,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            ManageReservationsController controller)
        {
            routeModel.UkPrn = null;
            viewModel.Manage = false;
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Id == routeModel.EmployerAccountId &&
                        parameters.Controller == "teams" &&
                        parameters.Folder == "accounts" &&
                        parameters.SubDomain == "accounts")))
                .Returns(expectedUrl);
            
            var result = controller.PostDeleteCompleted(routeModel, viewModel) as RedirectResult;

            result.Url.Should().Be(expectedUrl);
        }
    }
}