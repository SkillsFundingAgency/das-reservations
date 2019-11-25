using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
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
        private string _reviewRedirectUrl;

        public ProviderCreateReservationSteps(ProviderTestServiceProvider serviceProvider, TestData testData) : base(serviceProvider, testData)
        {
        }

        [Given(@"I am a provider")]
        public void GivenIAmAProvider()
        {
            SetupProviderTestData();
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
                AccountLegalEntityPublicHashedId = TestData.AccountLegalEntity.AccountLegalEntityPublicHashedId
            };

            TestData.ActionResult = controller.ProcessConfirmEmployer(confirmEmployerViewModel)
                .Result;
            
            if (typeof(RedirectToRouteResult) == TestData.ActionResult.GetType())
            {
                var result = TestData.ActionResult as RedirectToRouteResult;

                Assert.IsNotNull(result);
                Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining, result.RouteName);
            }
        }
    }
}