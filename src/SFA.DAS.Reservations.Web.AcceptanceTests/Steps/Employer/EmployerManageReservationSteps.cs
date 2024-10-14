using System;
using System.Linq;
using FluentAssertions;
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

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps.Employer
{
    [Binding]
    public class EmployerManageReservationSteps :StepsBase
    {
        private ManageViewModel _actualModel;

        public EmployerManageReservationSteps(EmployerTestServiceProvider serviceProvider, TestData testData) : base(serviceProvider, testData)
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

            for (var i = 0; i < numberOfReservations; i++)
            {
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
        }

        [When(@"I view the manage reservation screen")]
        public void WhenIViewTheManageReservationScreen()
        {
            var controller = Services.GetService<ManageReservationsController>();
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);
            mock.Setup(x => x.GetAll<GetReservationResponse>(
                It.IsAny<ReservationApiRequest>())).ReturnsAsync(TestData.Reservations);

            var actual = controller.EmployerManage(TestData.ReservationRouteModel).Result as ViewResult;
            actual.Should().NotBeNull();
            _actualModel = actual.Model as ManageViewModel;
        }

        [Then(@"(.*) reservations are displayed")]
        public void ThenReservationsAreDisplayed(int numberOfReservations)
        {
            _actualModel.Should().NotBeNull();

            _actualModel.Should().NotBeNull("View model has not been set");
            _actualModel.Reservations.Should().NotBeNull("Reservations have not been set");
            numberOfReservations.Should().Be(_actualModel.Reservations.Count);
        }

        [Then(@"I am able to delete the reservation")]
        public void ThenIAmAbleToDeleteTheReservation()
        {
            _actualModel.Reservations.First().CanProviderDeleteReservation.Should().BeTrue();
        }

        [Then(@"I am not able to delete the reservation")]
        public void ThenIAmNotAbleToDeleteTheReservation()
        {
            _actualModel.Reservations.First().DeleteRouteName.Should().BeEmpty();
        }

        [Then(@"I am able to add an apprentice")]
        public void ThenIAmAbleToAddAnApprentice()
        {
            _actualModel.Reservations.First().ApprenticeUrl.Should().NotBeEmpty();
        }

        [Then(@"I am not able to add an apprentice")]
        public void ThenIAmNotAbleToAddAnApprentice()
        {
            _actualModel.Reservations.First().ApprenticeUrl.Should().BeEmpty();
        }
    }
}
