﻿using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;

namespace SFA.DAS.Reservations.Web.UnitTests.Manage
{
    [TestFixture]
    public class WhenCallingPostDeleteCompleted
    {
        [Test, DomainAutoData]
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

        [Test, DomainAutoData]
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

        [Test, DomainAutoData]
        public void And_Has_Ukprn_And_Manage_True_Then_Redirects_To_Provider_Manage(
            ReservationsRouteModel routeModel,
            DeleteCompletedViewModel viewModel,
            ManageReservationsController controller)
        {
            viewModel.Manage = true;

            var result = controller.PostDeleteCompleted(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderManage);
        }

        [Test, DomainAutoData]
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

        [Test, DomainAutoData]
        public void And_Has_Ukprn_And_Manage_False_Then_Redirects_To_Provider_Dashboard(
            ReservationsRouteModel routeModel,
            DeleteCompletedViewModel viewModel,
            string providerDashboardUrl,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            ManageReservationsController controller)
        {
            routeModel.EmployerAccountId = null;
            externalUrlHelper.Setup(x => x.GenerateDashboardUrl(null)).Returns(providerDashboardUrl);
            viewModel.Manage = false;

            var result = controller.PostDeleteCompleted(routeModel, viewModel) as RedirectResult;

            result.Url.Should().Be(providerDashboardUrl);
        }

        [Test, DomainAutoData]
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
                .Setup(helper => helper.GenerateDashboardUrl(routeModel.EmployerAccountId))
                .Returns(expectedUrl);
            
            var result = controller.PostDeleteCompleted(routeModel, viewModel) as RedirectResult;

            result.Url.Should().Be(expectedUrl);
        }
    }
}