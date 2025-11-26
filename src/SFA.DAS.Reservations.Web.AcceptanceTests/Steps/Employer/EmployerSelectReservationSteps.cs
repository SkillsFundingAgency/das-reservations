using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using Reqnroll;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps.Employer
{
    [Binding]
    public class EmployerSelectReservationSteps : StepsBase
    {
        private SelectReservationViewModel _actualModel;
        private IActionResult _actionResult;
        private readonly SelectReservationViewModel _viewModel;

        public EmployerSelectReservationSteps(EmployerTestServiceProvider serviceProvider, TestData testData) : base(serviceProvider, testData)
        {
            _viewModel = new SelectReservationViewModel();
        }

        [Given(@"I have a transfer receiver")]
        public void GivenIHaveATransferReceiver()
        {
            const string transferSenderPublicHashedAccountId = "BBB222";

            _viewModel.TransferSenderId = transferSenderPublicHashedAccountId;
            _viewModel.CohortReference = TestDataValues.CohortReference;

            var reservationsClient = Services.GetService<IReservationsOuterService>();
            var mockReservations = Mock.Get(reservationsClient);
            mockReservations.Setup(x => x.GetTransferValidity(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int?>()))
                .ReturnsAsync(() => new GetTransferValidityResponse{ IsValid = true });
        }
        
        [Given(@"I am a levy employer")]
        public void GivenIAmALevyEmployer()
        {
            _viewModel.CohortReference = TestDataValues.CohortReference;

            SetupLevyEmployerTestData();
        }

        [Given(@"I have no cohort reference")]
        public void GivenIHaveNoCohortReference()
        {
            _viewModel.CohortReference = string.Empty;
            _viewModel.ProviderId = TestDataValues.ProviderId;
        }

        [Given(@"There are no more reservations available")]
        public void GivenThereAreNoReservationsAvailable()
        {
            _viewModel.CohortReference = string.Empty;
            _viewModel.ProviderId = TestDataValues.ProviderId;
        }


        [When(@"I view the select reservation screen")]
        public void WhenIViewTheSelectReservationScreen()
        {
            var controller = Services.GetService<SelectReservationsController>();
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, TestData.UserId.ToString());
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            mock.Setup(x => x.GetAll<GetReservationResponse>(
                It.IsAny<ReservationApiRequest>())).ReturnsAsync(TestData.Reservations);
            
            _actionResult = controller.SelectReservation(TestData.ReservationRouteModel, _viewModel).Result;

            var actual = _actionResult as ViewResult;
            _actualModel = actual?.Model as SelectReservationViewModel;
        }

        [Then(@"(.*) reservations are selectable")]
        public void ThenReservationsAreSelectable(int numberOfReservations)
        {
            _actualModel.Should().NotBeNull("View model has not been set");
            _actualModel.AvailableReservations.Should().NotBeNull("Reservations have not been set");
            _actualModel.AvailableReservations.Count().Should().Be(numberOfReservations);
        }

        
        [Then(@"I am redirected to the create reservation page")]
        public void ThenIAmRedirectedToTheCreateReservationPage()
        {
            var redirectResult = _actionResult as RedirectToRouteResult;

            redirectResult.Should().NotBeNull();
            redirectResult.RouteName.Should().Be(RouteNames.EmployerSelectCourseRuleCheck);
            redirectResult.RouteValues["AccountLegalEntityPublicHashedId"].Should().Be(TestData.ReservationRouteModel.AccountLegalEntityPublicHashedId);
            redirectResult.RouteValues["CohortReference"].Should().Be(TestData.ReservationRouteModel.CohortReference);
            redirectResult.RouteValues["EmployerAccountId"].Should().Be(TestData.ReservationRouteModel.EmployerAccountId);
        }
        
        [Then(@"I am redirected to the add apprentice page")]
        public void ThenIAmRedirectedToTheAddApprenticePage()
        {
            var redirectResult = _actionResult as RedirectResult;

            redirectResult.Should().NotBeNull();
            redirectResult.Url.StartsWith($"https://{TestDataValues.EmployerApprenticeUrl}/{TestData.ReservationRouteModel.EmployerAccountId}/unapproved/{TestDataValues.CohortReference}/apprentices/add?reservationId=").Should().BeTrue();

            VerifyAddApprenticeQueryParams(redirectResult);

            VerifyLevyReservationCreated();
        }

        [Then(@"I am redirected to the add apprentice page with no cohort ref")]
        public void ThenIAmRedirectedToTheAddApprenticePageWithNoCohortRef()
        {
            var redirectResult = _actionResult as RedirectResult;

            redirectResult.Should().NotBeNull();
            redirectResult.Url.StartsWith($"https://{TestDataValues.EmployerApprenticeUrl}/{TestData.ReservationRouteModel.EmployerAccountId}/unapproved/add/apprentice?").Should().BeTrue();
            var queryParams = new Uri(redirectResult.Url).ParseQueryString();
            queryParams["providerId"].Should().Be(TestDataValues.ProviderId.ToString());
            VerifyAddApprenticeQueryParams(redirectResult);

            VerifyLevyReservationCreated();
        }

        [Then(@"I am redirected to the add apprentice page with no cohort ref and no reservation")]
        public void ThenIAmRedirectedToTheAddApprenticePageWithNoCohortRefAndNoReservation()
        {
            var redirectResult = _actionResult as RedirectResult;

            redirectResult.Should().NotBeNull();
            redirectResult.Url.StartsWith($"https://{TestDataValues.EmployerApprenticeUrl}/{TestData.ReservationRouteModel.EmployerAccountId}/unapproved/add/apprentice?").Should().BeTrue();
            var queryParams = new Uri(redirectResult.Url).ParseQueryString();
            queryParams["providerId"].Should().Be(TestDataValues.ProviderId.ToString());
            VerifyAddApprenticeQueryParams(redirectResult, false);
        }

        [Then(@"I am redirected to message saying I have reached my reservation limit")]
        public void ThenIAmRedirectedToAPageSayingReachedReaservationLimit()
        {
            var viewResult = _actionResult as ViewResult;

            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be("ReservationLimitReached");
        }


        private void VerifyAddApprenticeQueryParams(RedirectResult redirectResult, bool expectReservationId = true)
        {
            var uri = new Uri(redirectResult.Url);
            var queryParams = uri.ParseQueryString();
            queryParams["autocreated"].Should().Be("true");
            queryParams["accountLegalEntityHashedId"].Should().Be(TestData.ReservationRouteModel.AccountLegalEntityPublicHashedId);
            queryParams["transferSenderId"].Should().Be(_viewModel.TransferSenderId);
            if (expectReservationId)
            {
                (Guid.Parse(queryParams["reservationId"]) != Guid.Empty).Should().BeTrue();
            }
        }

        private void VerifyLevyReservationCreated()
        {
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);
            mock.Verify(x => x.Create<CreateReservationResponse>(
                It.Is<ReservationApiRequest>(c => c.UserId.Equals(TestData.UserId)
                                                  && c.AccountLegalEntityId.Equals(TestData.AccountLegalEntity
                                                      .AccountLegalEntityId)
                                                  && c.AccountId.Equals(TestData.AccountLegalEntity.AccountId))));
        }
    }
}