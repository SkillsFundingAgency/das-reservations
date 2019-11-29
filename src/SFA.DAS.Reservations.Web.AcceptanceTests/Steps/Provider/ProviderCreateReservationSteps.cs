﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps.Provider
{
    [Binding]
    public class ProviderCreateReservationSteps : StepsBase
    {
        public ProviderCreateReservationSteps(
            ProviderTestServiceProvider serviceProvider, 
            TestData testData) 
            : base(serviceProvider, testData)
        {
        }

        [When(@"I choose an employer's account legal entity")]
        public void WhenIChooseAnEmployersAccountLegalEntity()
        {
            var controller = Services.GetService<ProviderReservationsController>();
            var urlHelper = Services.GetService<IUrlHelper>();
            var mock = Mock.Get(urlHelper);
            controller.Url = mock.Object;
            var confirmEmployerViewModel = new ConfirmEmployerViewModel
            {
                Confirm = true,
                AccountId = TestData.AccountLegalEntity.AccountId,
                AccountLegalEntityId = TestData.AccountLegalEntity.AccountLegalEntityId,
                AccountLegalEntityName = TestData.AccountLegalEntity.AccountLegalEntityName,
                AccountLegalEntityPublicHashedId = TestData.AccountLegalEntity.AccountLegalEntityPublicHashedId,
                UkPrn = TestData.ReservationRouteModel.UkPrn.Value
            };

            var result = controller.ProcessConfirmEmployer(confirmEmployerViewModel)
                .Result as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderApprenticeshipTraining);
            var routeValues = result.RouteValues;
            routeValues["Id"].GetType().Should().Be<Guid>();
            routeValues["Id"].Should().NotBe(Guid.Empty);
            routeValues["UkPrn"].Should().Be(confirmEmployerViewModel.UkPrn);

            TestData.ReservationRouteModel.Id = routeValues["Id"] as Guid? ?? default;
        }

        [When(@"I choose a course and date of (.*) on behalf of an employer")]
        public void WhenIChooseACourseAndDateOnBehalfOfAnEmployer(string month)
        {
            TestData.BuildTrainingDateModel(month);
            var trainingDateViewModel = new TrainingDateViewModel(TestData.TrainingDate);
            var postSelectCourseViewModel = new ApprenticeshipTrainingFormModel
            {
                AccountLegalEntityPublicHashedId = TestData.ReservationRouteModel.AccountLegalEntityPublicHashedId,
                SelectedCourseId = TestData.Course.Id,
                StartDate = trainingDateViewModel.SerializedModel
            };

            var controller = Services.GetService<ReservationsController>();

            var result = controller.PostApprenticeshipTraining(TestData.ReservationRouteModel, postSelectCourseViewModel)
                .Result as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.ProviderReview);
            result.RouteValues["Id"].Should().Be(TestData.ReservationRouteModel.Id);
            result.RouteValues["UkPrn"].Should().Be(TestData.ReservationRouteModel.UkPrn);
        }
    }
}