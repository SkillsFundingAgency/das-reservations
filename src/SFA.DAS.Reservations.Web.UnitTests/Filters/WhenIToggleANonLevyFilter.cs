using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntity;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.AppStart;
using SFA.DAS.Reservations.Web.Filters;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;
using RedirectToRouteResult = Microsoft.AspNetCore.Mvc.RedirectToRouteResult;

namespace SFA.DAS.Reservations.Web.UnitTests.Filters
{
    public class WhenIToggleANonLevyFilter
    {
        [Test, MoqAutoData]
        public async Task Then_Toggle_Is_Checked(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntityResponse legalEntityResponse,
            AccountLegalEntity legalEntity,
            string legalEntityHashedId,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            MockGetLegalEntityCall(mockMediator, legalEntityResponse, 
                legalEntity, legalEntityHashedId, decodedId, mockEncodingService, false);

            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            context.RouteData.Values.Add("accountLegalEntityPublicHashedId", legalEntityHashedId);

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
            GetLegalEntityResponse legalEntityResponse,
            AccountLegalEntity legalEntity,
            string legalEntityHashedId,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            MockGetLegalEntityCall(mockMediator, legalEntityResponse, 
                legalEntity, legalEntityHashedId, decodedId, mockEncodingService, true);

            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            context.RouteData.Values.Add("accountLegalEntityPublicHashedId", legalEntityHashedId);

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
                enc.Decode(It.IsAny<string>(), EncodingType.PublicAccountLegalEntityId), Times.Never);

            var redirect =  context.Result as RedirectToRouteResult;
            Assert.IsNull(redirect);

        }

        [Test, MoqAutoData]
        public async Task Then_If_Toggled_Off_And_Non_Levy_Request_Is_Redirected(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntityResponse legalEntityResponse,
            AccountLegalEntity legalEntity,
            string legalEntityHashedId,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            context.RouteData.Values.Add("accountLegalEntityPublicHashedId", legalEntityHashedId);

            MockGetLegalEntityCall(mockMediator, legalEntityResponse, 
                legalEntity, legalEntityHashedId, decodedId, mockEncodingService, false);
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");

            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);


            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect = context.Result as RedirectToRouteResult;

            //Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.EmployerFeatureNotAvailable, redirect.RouteName);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Toggled_Off_And_Cannot_Decode_Account_Id_Is_Redirected(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntityResponse legalEntityResponse,
            AccountLegalEntity legalEntity,
            string legalEntityHashedId,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            context.RouteData.Values.Add("accountLegalEntityPublicHashedId", legalEntityHashedId);
           
            legalEntityResponse.AccountLegalEntity = legalEntity;
            legalEntityResponse.AccountLegalEntity.IsLevy = false; 

            mockEncodingService
                .Setup(x => x.TryDecode(legalEntityHashedId, EncodingType.PublicAccountLegalEntityId, out decodedId))
                .Returns(false);
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");

            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);

            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect = context.Result as RedirectToRouteResult;

            //Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.EmployerFeatureNotAvailable, redirect.RouteName);
           
        }

         [Test, MoqAutoData]
        public async Task Then_If_Toggled_Off_And_Cannot_Get_Legal_Entity_Is_Redirected(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntityResponse legalEntityResponse,
            AccountLegalEntity legalEntity,
            string legalEntityHashedId,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            context.RouteData.Values.Add("accountLegalEntityPublicHashedId", legalEntityHashedId);
            legalEntityResponse.AccountLegalEntity = legalEntity;
            legalEntityResponse.AccountLegalEntity.IsLevy = false;

            mockEncodingService
                .Setup(x => x.TryDecode(legalEntityHashedId, EncodingType.PublicAccountLegalEntityId, out decodedId))
                .Returns(true);

            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntityQuery>(y => y.Id == decodedId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetLegalEntityResponse());
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");
           
            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);

            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect = context.Result as RedirectToRouteResult;

            //Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.EmployerFeatureNotAvailable, redirect.RouteName);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Toggled_Off_And_Levy_Request_Is_Not_Redirected(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntityResponse legalEntityResponse,
            AccountLegalEntity legalEntity,
            string legalEntityHashedId,
            string employerAccountId,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("employerAccountId", employerAccountId);
            context.RouteData.Values.Add("accountLegalEntityPublicHashedId", legalEntityHashedId);

            MockGetLegalEntityCall(mockMediator, legalEntityResponse, 
                legalEntity, legalEntityHashedId, decodedId, mockEncodingService, true); 
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");

            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);
            
            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect =  context.Result as RedirectToRouteResult;

            //Assert
            Assert.IsNull(redirect);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Redirected_Provider_is_Redirected_To_Correct_Route(
            [Frozen] Mock<IConfiguration> configuration, 
            [Frozen] ServiceParameters serviceParameters,
            [Frozen] Mock<IMediator> mockMediator,
            GetLegalEntityResponse legalEntityResponse,
            AccountLegalEntity legalEntity,
            string legalEntityHashedId,
            string ukprn,
            long decodedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<ActionExecutionDelegate> nextMethod,
            ActionExecutingContext context)
        {
            //Assign
            context.RouteData.Values.Add("ukprn", ukprn);
            context.RouteData.Values.Add("accountLegalEntityPublicHashedId", legalEntityHashedId);
            legalEntityResponse.AccountLegalEntity = legalEntity;
            legalEntityResponse.AccountLegalEntity.IsLevy = false;

            mockEncodingService
                .Setup(x => x.TryDecode(legalEntityHashedId, EncodingType.PublicAccountLegalEntityId, out decodedId))
                .Returns(true);

            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntityQuery>(y => y.Id == decodedId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetLegalEntityResponse());
            
            configuration.SetupGet(c => c["FeatureToggleOn"]).Returns("False");
           
            var filter = new NonLevyFeatureToggleActionFilter(
                configuration.Object, 
                serviceParameters, 
                mockEncodingService.Object, 
                mockMediator.Object);

            ////Act
            await filter.OnActionExecutionAsync(context,nextMethod.Object);

            var redirect = context.Result as RedirectToRouteResult;

            //Assert
            Assert.IsNotNull(redirect);
            Assert.AreEqual(RouteNames.ProviderFeatureNotAvailable, redirect.RouteName);
        }

        private static void MockGetLegalEntityCall(Mock<IMediator> mockMediator,
            GetLegalEntityResponse legalEntityResponse, AccountLegalEntity legalEntity, 
            string legalEntityHashedId, long decodedId, Mock<IEncodingService> mockEncodingService, bool isLevy)
        {
            legalEntity.IsLevy = isLevy;
            legalEntityResponse.AccountLegalEntity = legalEntity;

            mockEncodingService
                .Setup(x => x.TryDecode(legalEntityHashedId, EncodingType.PublicAccountLegalEntityId, out decodedId))
                .Returns(true);

            mockMediator
                .Setup(x => x.Send(It.Is<GetLegalEntityQuery>(ale => ale.Id == decodedId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalEntityResponse);
        }
    }
}
