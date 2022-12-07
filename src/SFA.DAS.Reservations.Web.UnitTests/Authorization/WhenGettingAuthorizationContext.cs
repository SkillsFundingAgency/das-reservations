using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Web.Authorization;

namespace SFA.DAS.Reservations.Web.UnitTests.Authorization
{
    public class WhenGettingAuthorizationContext
    {
        private const long CohortId = 12345;
        private const long UkPrn = 4444;
        private const long EmployerAccountId = 22;

        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private Mock<IEncodingService> _encodingService;
        private AuthorizationContextProvider _contextProvider;
        private RouteData _routeData;
        private Mock<IRoutingFeature> _routingFeature;

        private delegate void TryGetCallback(string cohortRef, EncodingType encodingType, ref long val);
        private delegate bool TryGetReturns(string cohortRef, EncodingType encodingType, ref long val);


        [SetUp]
        public void Arrange()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _encodingService = new Mock<IEncodingService>();
            _routingFeature = new Mock<IRoutingFeature>();
            _routeData = new RouteData();
            _routeData.Values.Add("ukprn", UkPrn);

            _contextProvider = new AuthorizationContextProvider(_httpContextAccessor.Object, _encodingService.Object);

            _encodingService
                .Setup(x => x.TryDecode(It.IsAny<string>(), EncodingType.CohortReference, out It.Ref<long>.IsAny))
                .Callback(new TryGetCallback((string cohortRef, EncodingType encodingType, ref long val) => val = CohortId))
                .Returns(new TryGetReturns((string cohortRef, EncodingType encodingType, ref long val) => true));

            _routingFeature.Setup(f => f.RouteData).Returns(_routeData);

            //_httpContextAccessor.Setup(c => c.HttpContext.Features[typeof(IRoutingFeature)])
            //    .Returns(_routingFeature.Object);

            _httpContextAccessor.Setup(c => c.HttpContext.Features.Get<IRoutingFeature>())
                .Returns(_routingFeature.Object);

            var queryParams = new Dictionary<string, StringValues>
            {
                {"cohortRef", new StringValues(CohortId.ToString())}
            };

            _httpContextAccessor.Setup(c => c.HttpContext.Request.Query).Returns(new QueryCollection(queryParams));
        }

        [Test]
        public void ThenAddsAccountIdToContext()
        {
            //Arrange
            _routeData.Values.Remove("ukprn");
            _routeData.Values.Add("employerAccountId", EmployerAccountId);

            _encodingService
                .Setup(x => x.TryDecode(It.IsAny<string>(), EncodingType.AccountId, out It.Ref<long>.IsAny))
                .Callback(new TryGetCallback((string accountId, EncodingType encodingType, ref long val) => val = EmployerAccountId))
                .Returns(new TryGetReturns((string accountId, EncodingType encodingType, ref long val) => true));

            //Act
            var context = _contextProvider.GetAuthorizationContext();

            //Assert
            Assert.IsTrue(context.TryGet<long>("PartyId", out var actualAccountId));
            Assert.AreEqual(EmployerAccountId, actualAccountId);
        }

        [Test]
        public void ThenAddsCohortIdToContext()
        {
            //Act
            var context = _contextProvider.GetAuthorizationContext();

            //Assert
            Assert.IsTrue(context.TryGet<long>("CohortId", out var actualCohortId));
            Assert.AreEqual(CohortId, actualCohortId);
        }

        [Test]
        public void ThenAddsUkPrnToContext()
        {
            //Act
            var context = _contextProvider.GetAuthorizationContext();

            //Assert
            Assert.IsTrue(context.TryGet<long>("PartyId", out var actualUkPrn));
            Assert.AreEqual(UkPrn, actualUkPrn);
        }

     

        [Test]
        public void ThenIfAnEmptyCohortIsAvailableItWillNotBeCollected()
        {
            //Arrange
            var queryParams = new Dictionary<string, StringValues>
            {
                {"cohortRef", new StringValues("")}
            };

            var queryCollection = new QueryCollection(queryParams);
            _httpContextAccessor.Setup(c => c.HttpContext.Request.Query).Returns(queryCollection);

            //Act
            var context = _contextProvider.GetAuthorizationContext();

            //Assert
            _encodingService.Verify(x => 
                x.TryDecode(It.IsAny<string>(), EncodingType.CohortReference, out It.Ref<long>.IsAny), Times.Never);

            Assert.IsFalse(context.TryGet<long>("CohortId", out var actualCohortId));
        }

        [Test]
        public void ThenThrowsUnauthorizedExceptionIfCohortIsNotValid()
        {
            //Arrange
            _encodingService
                .Setup(x => x.TryDecode(It.IsAny<string>(), EncodingType.CohortReference, out It.Ref<long>.IsAny))
                .Returns(false);

            var queryParams = new Dictionary<string, StringValues>
            {
                {"cohortRef", new StringValues("Not Valid")}
            };

            var queryCollection = new QueryCollection(queryParams);
            _httpContextAccessor.Setup(c => c.HttpContext.Request.Query).Returns(queryCollection);

            //Act
            Assert.Throws<UnauthorizedAccessException>(() => _contextProvider.GetAuthorizationContext());
        }

        [Test]
        public void ThenThrowsUnauthorizedExceptionIfUkPrnIsNotValid()
        {
            //Arrange
            _routeData.Values.Clear();
            _routeData.Values.Add("ukprn", "Not Valid");

            //Act + Assert
            Assert.Throws<UnauthorizedAccessException>(() => _contextProvider.GetAuthorizationContext());
        }

        [Test]
        public void ThenThrowsUnauthorizedExceptionIfEmployerAccountIdIsNotValid()
        {
            //Arrange
            _routeData.Values.Clear();
            _routeData.Values.Add("employerAccountId", "Not Valid");

            //Act + Assert
            Assert.Throws<UnauthorizedAccessException>(() => _contextProvider.GetAuthorizationContext());
        }

        [Test]
        public void ThenDoesNotAddPermissionIfCohortIsNotAvailable()
        {
            //Arrange
            _httpContextAccessor.Setup(c => c.HttpContext.Request.Query).Returns(new QueryCollection());

            //Act
            var context = _contextProvider.GetAuthorizationContext();

            //Assert
            Assert.IsFalse(context.TryGet<long>("CohortId", out var x));
            Assert.IsFalse(context.TryGet<long>("PartyId", out var x1));
        }

        [Test]
        public void ThenDoesNotAddPermissionIfUkPrnIsNotAvailable()
        {
            //Arrange
            _routeData.Values.Clear();

            //Act
            var context = _contextProvider.GetAuthorizationContext();

            //Assert
            Assert.IsFalse(context.TryGet<long>("PartyId", out var x));
            Assert.IsFalse(context.TryGet<long>("CohortId", out var x1));
        }
    }

}
