using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetApprenticeshipTraining
    {
        [Test, MoqAutoData]
        public async Task Then_It_Calls_Start_Date_Service_To_Get_Start_Dates(
            ReservationsRouteModel routeModel,
            IEnumerable<StartDateModel> expectedStartDates,
            [Frozen] Mock<IStartDateService> mockStartDateService,
            ReservationsController controller)
        {
            mockStartDateService
                .Setup(service => service.GetStartDates())
                .ReturnsAsync(expectedStartDates);
   
            await controller.ApprenticeshipTraining(routeModel);

            mockStartDateService.Verify(provider => provider.GetStartDates(), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_Apprenticeship_Training_View_With_Mapped_Dates(
            ReservationsRouteModel routeModel,
            IEnumerable<StartDateModel> expectedStartDates,
            [Frozen] Mock<IStartDateService> mockStartDateService,
            ReservationsController controller)
        {
            mockStartDateService
                .Setup(service => service.GetStartDates())
                .ReturnsAsync(expectedStartDates);
            var mappedDates = expectedStartDates.Select(startDateModel => new StartDateViewModel
            {
                Id = startDateModel.StartDate.ToString("yyyy-MM"),
                Value = JsonConvert.SerializeObject(startDateModel),
                Label = startDateModel.StartDate.ToString("MMMM yyyy")
            }).OrderBy(model => model.Value);
            
            var result = await controller.ApprenticeshipTraining(routeModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ApprenticeshipTrainingViewModel>()
                .Subject;
            
            viewModel.PossibleStartDates.Should().BeEquivalentTo(mappedDates);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Previous_Reservation_Then_Loads_Existing_Reservation_To_ViewModel(
            ReservationsRouteModel routeModel,
            IEnumerable<StartDateModel> expectedStartDates,
            [Frozen] Mock<IStartDateService> mockStartDateService,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {          
            await controller.ApprenticeshipTraining(routeModel);

            mockMediator.Verify(mediator => mediator.Send(It.Is<GetCachedReservationQuery>(query => query.Id == routeModel.Id), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_No_Previous_Reservation_Then_Not_Load_Existing_Reservation(
            ReservationsRouteModel routeModel,
            IEnumerable<StartDateModel> expectedStartDates,
            [Frozen] Mock<IStartDateService> mockStartDateService,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            routeModel.Id = null;

            await controller.ApprenticeshipTraining(routeModel);

            mockMediator.Verify(mediator => mediator.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}