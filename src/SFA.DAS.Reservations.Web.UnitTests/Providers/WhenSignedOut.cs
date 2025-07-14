using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers;

[TestFixture]
public class WhenSignedOut
{
    [Test, MoqAutoData]
    public void And_Is_AutoSignOut_Then_Return_AutoSignOut_View_With_ViewModel(
        string redirectUrl,
        [Frozen] ReservationsWebConfiguration configuration,
        [NoAutoProperties] HomeController controller)
    {
        // Arrange
        configuration.DashboardUrl = redirectUrl;
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        controller.TempData["AutoSignOut"] = true;

        // Act
        var result = controller.ProviderSignedOut() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result.ViewName.Should().Be("AutoSignOut");
        var model = result.Model as AutoSignOutViewModel;
        model.Should().NotBeNull();
        model.ProviderPortalBaseUrl.Should().Be(redirectUrl);
    }

    [Test, MoqAutoData]
    public void Then_Redirect_To_Provider_Dashboard(
        string redirectUrl,
        [Frozen] ReservationsWebConfiguration configuration,
        [NoAutoProperties] HomeController controller)
    {
        // Arrange
        configuration.DashboardUrl = redirectUrl;
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        controller.TempData["AutoSignOut"] = false;

        // Act
        var result = controller.ProviderSignedOut() as RedirectResult;

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Be(redirectUrl);
    }
}