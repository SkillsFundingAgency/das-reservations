using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingPostSelectReservation
    {
        [Test, MoqAutoData]
        public void And_Has_Ukprn_And_CreateNew_Then_Redirects_To_ProviderStart()
        {
            //todo: story for create new
        }

        [Test, MoqAutoData]
        public void And_Has_Ukprn_And_ReservationId_Then_Redirects_To_AddApprentice(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            string addApprenticeUrl,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            ReservationsController controller)
        {
            viewModel.CreateNew = null;
            mockUrlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(
                    It.Is<UrlParameters>(parameters => 
                        parameters.Folder == routeModel.UkPrn.ToString() &&
                        parameters.Id == "unapproved" &&
                        parameters.Controller == viewModel.CohortReference &&
                        parameters.Action == "apprentices/add" &&
                        parameters.QueryString == $"?reservationId={viewModel.SelectedReservationId}")))
                .Returns(addApprenticeUrl);
            
            var result = controller.PostSelectReservation(routeModel, viewModel) as RedirectResult;

            result.Url.Should().Be(addApprenticeUrl);
        }

        [Test, MoqAutoData]
        public void And_No_Create_And_No_ReservationId_Then_Returns_Error500(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            ReservationsController controller)
        {
            viewModel.CreateNew = null;
            viewModel.SelectedReservationId = null;
            
            var result = controller.PostSelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }
    }
}