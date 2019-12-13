using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Filters
{
    public class WhenIToggleANonLevyFilter
    {
        [Test, MoqAutoData]
        public async Task Then_Toggle_Is_Checked(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            MockGetLegalEntityCall(serviceParameters, mockMediator, legalEntitiesResponse, 
                legalEntities, employerAccountId, decodedId, mockEncodingService, false);

            context.RouteData.Values.Add("employerAccountId", employerAccountId);

            configuration.Setup(c => c["FeatureToggleOn"]).Returns("True");

            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);

            //Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            //Assert
            configuration.Verify(c => c["FeatureToggleOn"], Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Employer_Is_Not_Checked_For_Levy_If_Toggle_Is_On(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            MockGetLegalEntityCall(serviceParameters, mockMediator, legalEntitiesResponse, 
                legalEntities, employerAccountId, decodedId, mockEncodingService, true);

            context.RouteData.Values.Add("employerAccountId", employerAccountId);

            configuration.Setup(c => c["FeatureToggleOn"]).Returns("True");

            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);

            //Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            //Assert
            mockEncodingService.Verify(enc => 
                enc.Decode(It.IsAny<string>(), EncodingType.AccountId), Times.Never);

            var redirect =  context.Result as RedirectToActionResult;
            Assert.IsNull(redirect);

        }

        [Test, MoqAutoData]
        public async Task Then_If_Toggled_Off_And_Non_Levy_Request_Is_Redirected(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("employerAccountId", employerAccountId);

            MockGetLegalEntityCall(serviceParameters, mockMediator, legalEntitiesResponse, 
                legalEntities, employerAccountId, decodedId, mockEncodingService, false);
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");

            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);


            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect = context.Result as RedirectToActionResult;

            //Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Home", redirect.ControllerName);
            Assert.AreEqual("FeatureNotAvailable", redirect.ActionName);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Toggled_Off_And_Cannot_Decode_Account_Id_Is_Redirected(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("employerAccountId", employerAccountId);

            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            legalEntitiesResponse.AccountLegalEntities = legalEntities;
           
            foreach (var legalEntity in legalEntitiesResponse.AccountLegalEntities)
            {
                legalEntity.IsLevy = false;
            }

            mockEncodingService
                .Setup(x => x.TryDecode(employerAccountId, EncodingType.AccountId, out decodedId))
                .Returns(false);
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");

            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);

            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect = context.Result as RedirectToActionResult;

            //Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Home", redirect.ControllerName);
            Assert.AreEqual("FeatureNotAvailable", redirect.ActionName);
        }

         [Test, MoqAutoData]
        public async Task Then_If_Toggled_Off_And_Cannot_Get_Any_Legal_Entities_Is_Redirected(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            legalEntitiesResponse.AccountLegalEntities = legalEntities;
           
            foreach (var legalEntity in legalEntitiesResponse.AccountLegalEntities)
            {
                legalEntity.IsLevy = false;
            }

            mockEncodingService
                .Setup(x => x.TryDecode(employerAccountId, EncodingType.AccountId, out decodedId))
                .Returns(true);

            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntitiesQuery>(y => y.AccountId == decodedId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetLegalEntitiesResponse(){AccountLegalEntities = new AccountLegalEntity[0]});
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");
           
            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);

            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect = context.Result as RedirectToActionResult;

            //Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Home", redirect.ControllerName);
            Assert.AreEqual("FeatureNotAvailable", redirect.ActionName);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Toggled_Off_And_Levy_Request_Is_Not_Redirected(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse,
            IEnumerable<AccountLegalEntity> legalEntities,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("employerAccountId", employerAccountId);

            MockGetLegalEntityCall(serviceParameters, mockMediator, legalEntitiesResponse, 
                legalEntities, employerAccountId, decodedId, mockEncodingService, true); 
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");

            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);


            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect =  context.Result as RedirectToActionResult;

            //Assert
            Assert.IsNull(redirect);
        }

        private static void MockGetLegalEntityCall(ServiceParameters serviceParameters, Mock<IMediator> mockMediator,
            GetLegalEntitiesResponse legalEntitiesResponse, IEnumerable<AccountLegalEntity> legalEntities, 
            string employerAccountId, long decodedId, Mock<IEncodingService> mockEncodingService, bool isLevy)
        {
            serviceParameters.AuthenticationType = AuthenticationType.Employer;
            legalEntitiesResponse.AccountLegalEntities = legalEntities;
           
            foreach (var legalEntity in legalEntitiesResponse.AccountLegalEntities)
            {
                legalEntity.IsLevy = isLevy;
            }

            mockEncodingService
                .Setup(x => x.TryDecode(employerAccountId, EncodingType.AccountId, out decodedId))
                .Returns(true);

            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntitiesQuery>(y => y.AccountId == decodedId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntitiesResponse);
        }
    }
}
