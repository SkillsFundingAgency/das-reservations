﻿using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenVisitingTheAccessDeniedPage
    {
        private Mock<IConfiguration> _configuration;
        private Mock<IOptions<ReservationsWebConfiguration>> _reservationsConfiguration;
        private bool _useDfESignIn;
        public ErrorController Sut { get; set; }

        [Test]
        [TestCase("test", "https://test-services.signin.education.gov.uk/organisations")]
        [TestCase("pp", "https://test-services.signin.education.gov.uk/organisations")]
        [TestCase("local", "https://test-services.signin.education.gov.uk/organisations")]
        [TestCase("prd", "https://services.signin.education.gov.uk/organisations")]
        public void ThenReturnsTheAccessDeniedModel(string env, string helpLink)
        {
            var fixture = new Fixture();
            _useDfESignIn = fixture.Create<bool>();

            fixture.Customize<ReservationsWebConfiguration>(c => c.With(x =>x.UseDfESignIn, _useDfESignIn));
            var mockReservationsConfig = fixture.Create<ReservationsWebConfiguration>();

            _configuration = new Mock<IConfiguration>();
            _reservationsConfiguration = new Mock<IOptions<ReservationsWebConfiguration>>();

            _configuration.Setup(x => x["ResourceEnvironmentName"]).Returns(env);
            _reservationsConfiguration.Setup(ap => ap.Value).Returns(mockReservationsConfig);

            Sut = new ErrorController(_configuration.Object, _reservationsConfiguration.Object);

            var result = (ViewResult)Sut.AccessDenied();

            Assert.That(result, Is.Not.Null);
            var actualModel = result?.Model as Error403ViewModel;
            Assert.That(actualModel?.HelpPageLink, Is.EqualTo(helpLink));
            Assert.AreEqual(actualModel?.UseDfESignIn, _useDfESignIn);
        }
    }
}
