using System;
using System.Threading;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
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
        [Test, MoqAutoData, Ignore("dan will finish it!")]
        public void And_Has_Ukprn_And_CreateNew_Then_Redirects_To_ProviderApprenticeshipTraining(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            Guid expectedReservationId = Guid.Empty;
            viewModel.CreateNew = true;
            mockMediator
                .Setup(mediator => mediator.Send(It.Is<CacheReservationEmployerCommand>(command =>
                    command.AccountLegalEntityPublicHashedId ==
                    routeModel.AccountLegalEntityPublicHashedId &&
                    command.UkPrn == routeModel.UkPrn), It.IsAny<CancellationToken>()))
                .Callback((CacheReservationEmployerCommand command) =>
                    expectedReservationId = command.Id);

            var result = controller.PostSelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderApprenticeshipTraining);
            result.RouteValues["ukPrn"].Should().Be(routeModel.UkPrn);
            result.RouteValues["id"].Should().Be(expectedReservationId);
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