using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps
{
    [Binding]
    public class ManageReservationSteps :StepsBase
    {
        private ManageViewModel _actualModel;

        public ManageReservationSteps(TestServiceProvider serviceProvider, TestData testData) : base(serviceProvider, testData)
        {
           
        }

        [Given(@"I have (.*) (.*) reservation")]
        public void GivenIHaveNumberOfReservations(int numberOfReservations, string status)
        {
            var parsedReservationStatusResult = Enum.TryParse(status, true, out ReservationStatus reservationStatus);
            var startDate = DateTime.UtcNow.AddMonths(1);
            var expiryDate = DateTime.UtcNow.AddMonths(3);
            var isExpired = false;

            if (!parsedReservationStatusResult)
            {
                isExpired = true;
                reservationStatus = ReservationStatus.Pending;
                startDate = DateTime.UtcNow.AddMonths(-3);
                expiryDate = DateTime.UtcNow.AddMonths(-1);
            }

            TestData.Reservations.Add(
                new GetReservationResponse
                {
                    Id = Guid.NewGuid(),
                    AccountLegalEntityId = TestData.AccountLegalEntity.AccountLegalEntityId,
                    StartDate = startDate,
                    ExpiryDate = expiryDate,
                    Course = new Course("1", "Test Course", 1),
                    AccountLegalEntityName = TestData.AccountLegalEntity.AccountLegalEntityName,
                    IsExpired = isExpired,
                    Status = (int)reservationStatus,
                    CreatedDate = DateTime.UtcNow
                }
            );
        }

        [When(@"I view the manage reservation screen")]
        public void WhenIViewTheManageReservationScreen()
        {
            var controller = Services.GetService<ManageReservationsController>();
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);
            mock.Setup(x => x.GetAll<GetReservationResponse>(
                It.IsAny<ReservationApiRequest>())).ReturnsAsync(TestData.Reservations);

            var actual = controller.Manage(TestData.ReservationRouteModel, TestData.FilterModel).Result as ViewResult;
            Assert.IsNotNull(actual);
            _actualModel = actual.Model as ManageViewModel;
        }

        [Then(@"(.*) reservations are displayed")]
        public void ThenReservationsAreDisplayed(int numberOfReservations)
        {
            Assert.IsNotNull(_actualModel, "View model has not been set");
            Assert.IsNotNull(_actualModel.Reservations, "Reservations have not been set");
            Assert.AreEqual(numberOfReservations, _actualModel.Reservations.Count);
        }

        [Then(@"I am able to delete the reservation")]
        public void ThenIAmAbleToDeleteTheReservation()
        {
            Assert.IsTrue(_actualModel.Reservations.First().CanProviderDeleteReservation);
        }

        [Then(@"I am not able to delete the reservation")]
        public void ThenIAmNotAbleToDeleteTheReservation()
        {
            Assert.AreEqual(string.Empty, _actualModel.Reservations.First().DeleteRouteName);
        }

        [Then(@"I am able to add an apprentice")]
        public void ThenIAmAbleToAddAnApprentice()
        {
            Assert.AreNotEqual(string.Empty, _actualModel.Reservations.First().ApprenticeUrl);
        }

        [Then(@"I am not able to add an apprentice")]
        public void ThenIAmNotAbleToAddAnApprentice()
        {
            Assert.AreEqual(string.Empty, _actualModel.Reservations.First().ApprenticeUrl);
        }


    }
}
