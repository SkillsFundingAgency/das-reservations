using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
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
            IEnumerable<DateTime> expectedStartDates,
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
        public async Task Then_It_Returns_The_Apprenticeship_Training_View(
            ReservationsRouteModel routeModel,
            IEnumerable<DateTime> expectedStartDates,
            [Frozen] Mock<IStartDateService> mockStartDateService,
            ReservationsController controller)
        {
            mockStartDateService
                .Setup(service => service.GetStartDates())
                .ReturnsAsync(expectedStartDates);
            var mappedDates = expectedStartDates.Select(date => new StartDateViewModel
            {
                Value = date.ToString("yyyy-MM"), 
                Label = date.ToString("MMMM yyyy")
            }).OrderBy(model => model.Value);

            var result = await controller.ApprenticeshipTraining(routeModel);
            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ApprenticeshipTrainingViewModel>()
                .Subject;
            
            viewModel.PossibleStartDates.Should().BeEquivalentTo(mappedDates);
            viewModel.RouteModel.Should().BeSameAs(routeModel);
        }
    }
}