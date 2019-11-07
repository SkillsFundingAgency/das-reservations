﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps
{
    [Binding]
    public class EmployerSelectReservationSteps : StepsBase
    {
        private SelectReservationViewModel _actualModel;
        private IActionResult _actionResult;
        private readonly SelectReservationViewModel _viewModel;

        public EmployerSelectReservationSteps(TestServiceProvider serviceProvider, TestData testData) : base(serviceProvider, testData)
        {
            _viewModel = new SelectReservationViewModel();
        }

        [Given(@"I have a transfer receiver")]
        public void GivenIHaveATransferReceiver()
        {
            const int transferSenderAccountId = 11;
            const string transferSenderHashedAccountId = "AAA555";
            const string transferSenderPublicHashedAccountId = "BBB222";

            _viewModel.TransferSenderId = transferSenderPublicHashedAccountId;
            _viewModel.CohortReference = TestDataValues.CohortReference;

            var accountApiClient = Services.GetService<IAccountApiClient>();
            var mock = Mock.Get(accountApiClient);

            mock.Setup(x => x.GetTransferConnections(It.IsAny<string>()))
                .ReturnsAsync(new List<TransferConnectionViewModel>
                {
                    new TransferConnectionViewModel
                    {
                        FundingEmployerAccountId = transferSenderAccountId,
                        FundingEmployerAccountName = "Sender Transfer Account",
                        FundingEmployerHashedAccountId = transferSenderHashedAccountId,
                        FundingEmployerPublicHashedAccountId = transferSenderPublicHashedAccountId
                    }
                });
        }
        
        [Given(@"I am a levy employer")]
        public void GivenIAmALevyEmployer()
        {
            SelectedAccountId = TestDataValues.LevyAccountId;
            SelectedHashedAccountId = TestDataValues.LevyHashedAccountId;
            _viewModel.CohortReference = TestDataValues.CohortReference;

            SetTestData();
        }

        [Given(@"I have no cohort reference")]
        public void GivenIHaveNoCohortReference()
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
            Assert.IsNotNull(_actualModel, "View model has not been set");
            Assert.IsNotNull(_actualModel.AvailableReservations, "Reservations have not been set");
            Assert.AreEqual(numberOfReservations, _actualModel.AvailableReservations.Count());
        }

        
        [Then(@"I am redirected to the create reservation page")]
        public void ThenIAmRedirectedToTheCreateReservationPage()
        {
            var redirectResult = _actionResult as RedirectToRouteResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(RouteNames.EmployerSelectCourseRuleCheck, redirectResult.RouteName);
            Assert.AreEqual(TestData.ReservationRouteModel.AccountLegalEntityPublicHashedId, redirectResult.RouteValues["AccountLegalEntityPublicHashedId"]);
            Assert.AreEqual(TestData.ReservationRouteModel.CohortReference, redirectResult.RouteValues["CohortReference"]);
            Assert.AreEqual(TestData.ReservationRouteModel.EmployerAccountId, redirectResult.RouteValues["EmployerAccountId"]);
        }
        
        [Then(@"I am redirected to the add apprentice page")]
        public void ThenIAmRedirectedToTheAddApprenticePage()
        {
            var redirectResult = _actionResult as RedirectResult;

            Assert.IsNotNull(redirectResult);
            Assert.IsTrue(redirectResult.Url.StartsWith($"https://{TestDataValues.EmployerApprenticeUrl}/{SelectedHashedAccountId}/unapproved/{TestDataValues.CohortReference}/apprentices/add?reservationId="));

            VerifyAddApprenticeQueryParams(redirectResult);

            VerifyLevyReservationCreated();
        }

        [Then(@"I am redirected to the add apprentice page with no cohort ref")]
        public void ThenIAmRedirectedToTheAddApprenticePageWithNoCohortRef()
        {
            var redirectResult = _actionResult as RedirectResult;

            Assert.IsNotNull(redirectResult);
            Assert.IsTrue(redirectResult.Url.StartsWith($"https://{TestDataValues.EmployerApprenticeUrl}/{SelectedHashedAccountId}/unapproved/add/apprentice?"));
            var queryParams = new Uri(redirectResult.Url).ParseQueryString();
            Assert.AreEqual(TestDataValues.ProviderId.ToString(), queryParams["providerId"]);
            VerifyAddApprenticeQueryParams(redirectResult);

            VerifyLevyReservationCreated();
        }

        private void VerifyAddApprenticeQueryParams(RedirectResult redirectResult)
        {
            var uri = new Uri(redirectResult.Url);
            var queryParams = uri.ParseQueryString();
            Assert.AreEqual("true", queryParams["autocreated"]);
            Assert.AreEqual(TestData.ReservationRouteModel.AccountLegalEntityPublicHashedId, queryParams["accountLegalEntityHashedId"]);
            Assert.IsTrue(Guid.Parse(queryParams["reservationId"]) != Guid.Empty);
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
