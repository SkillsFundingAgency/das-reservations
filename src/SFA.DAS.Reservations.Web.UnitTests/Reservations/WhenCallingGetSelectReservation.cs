using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Reservations
{
    [TestFixture]
    public class WhenCallingGetSelectReservation
    {
        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_ValidationException_Then_Redirect_To_ProviderError(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            Employer employer,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller)
        {
            
            routeModel.AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetProviderCacheReservationCommandQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Exception_Then_Redirect_To_ProviderError(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            string accountLegalEntityPublicHashedId,
            Exception exception,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller)
        {
            routeModel.AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
            
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Employer_And_Legal_Entity_Not_Found_Then_Redirect_To_Error(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller,
            [Frozen]AccountLegalEntity accountLegalEntity,
            GetAvailableReservationsResult reservationsResult)
        {
            routeModel.AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId;

            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AccountLegalEntityNotFoundException(routeModel.AccountLegalEntityPublicHashedId));
          
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error404);

            mockMediator
                .Verify(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_EmployerNot_Found_And_Legal_Entity_Id_Invalid_Then_Redirect_To_Error(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller,
            [Frozen]AccountLegalEntity accountLegalEntity,
            GetAvailableReservationsResult reservationsResult)
        {
            routeModel.AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId;

            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AccountLegalEntityInvalidException("Test error"));

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);

            mockMediator
                .Verify(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        }


        [Test, MoqAutoData]
        public async Task And_Has_AccountId_And_Employer_Not_Found_Then_Redirect_To_Error_Page(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetLegalEntitiesResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller)
        {
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetLegalEntitiesQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error404);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Gets_Reservations_For_Employer_Account(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            Employer employer,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller)
        {
            routeModel.AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = employer.AccountId,
                        AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = employer.AccountLegalEntityId,
                        AccountLegalEntityName = employer.AccountLegalEntityName,
                        AccountName = employer.AccountName,
                        CohortRef = routeModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            await controller.SelectReservation(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetAvailableReservationsQuery>(query => query.AccountId == employer.AccountId),
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_The_Back_Link_Is_Generated_From_The_UrlHelper(
            string cohortDetailsUrl,
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            Employer employer,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller)
        {
            //Arrange
            routeModel.AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = employer.AccountId,
                        AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = employer.AccountLegalEntityId,
                        AccountLegalEntityName = employer.AccountLegalEntityName,
                        AccountName = employer.AccountName,
                        CohortRef = routeModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);
            mockUrlHelper
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId,
                    viewModel.CohortReference, false, viewModel.JourneyData, string.Empty))
                .Returns(cohortDetailsUrl);

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            result.ViewName.Should().Be(ViewNames.Select);
            var actualModel = result.Model as SelectReservationViewModel;
            actualModel.Should().NotBeNull();
            actualModel.CohortReference.Should().Be(viewModel.CohortReference);
            actualModel.TransferSenderId.Should().Be(viewModel.TransferSenderId);
            actualModel.BackLink.Should().Be(cohortDetailsUrl);
            actualModel.AvailableReservations.Should().BeEquivalentTo(
                reservationsResult.Reservations
                    .Select(reservation => new AvailableReservationViewModel(reservation)));
        }

         
        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployer_ThenCreatesReservationInTheBackground(
            ReservationsRouteModel routeModel,
            Employer employer,
            GetLegalEntitiesResponse employersResponse,
            long expectedAccountId,
            Guid? expectedUserId,
            string addAprrenticeUrl,
            SelectReservationViewModel viewModel,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            [Frozen]Mock<IEncodingService> encodingService,
            [Frozen]Mock<IMediator> mediator,
            [Frozen]Mock<IExternalUrlHelper> urlHelper,
            SelectReservationsController controller
        )
        {
            //Arrange
            employer.AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId;
            routeModel.UkPrn = null;
            employer.AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId;
            employer.AccountId = expectedAccountId;
            viewModel.TransferSenderId = string.Empty;
            viewModel.EncodedPledgeApplicationId = "";
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId.ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mediator
                .Setup(m => m.Send(
                    It.Is<GetLegalEntitiesQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            urlHelper.Setup(x => x.GenerateAddApprenticeUrl(It.IsAny<Guid>(),
                routeModel.AccountLegalEntityPublicHashedId, It.IsAny<string>(), viewModel.ProviderId, It.IsAny<DateTime?>(),
                viewModel.CohortReference, routeModel.EmployerAccountId, false,"", "",
                It.IsAny<string>())).Returns(addAprrenticeUrl);
            createReservationLevyResult.ReservationId = Guid.NewGuid();
            mediator.Setup(
                    x => x.Send(
                        It.Is<CreateReservationLevyEmployerCommand>(query => query.AccountId == employer.AccountId 
                                                                    && query.UserId.Value == expectedUserId.Value), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);
            
            //Act   
            var actual = await controller.SelectReservation(routeModel, viewModel);


            //Assert
            Assert.IsNotNull(actual);
            var actualResult = actual as RedirectResult;
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(addAprrenticeUrl, actualResult.Url);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Transfer_Sender_Which_Is_Not_Valid_Goes_To_Error(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            [Frozen]Mock<IMediator> mediator,
            SelectReservationsController controller
        )
        {
            //Arrange
            mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), CancellationToken.None))
                .ThrowsAsync(new TransferSenderNotAllowedException(1, "1"));

            //Act   
            var result = await controller.SelectReservation(routeModel, viewModel);


            //Assert
            Assert.IsNotNull(result);
            var actualRedirectResult = result as RedirectToRouteResult;
            Assert.IsNotNull(actualRedirectResult);
            Assert.AreEqual(actualRedirectResult.RouteName, RouteNames.Error500);

        }


        [Test, MoqAutoData]
        public async Task And_Has_Transfer_Sender_Which_Is_Valid_Goes_To_Create_Levy_Reservation_And_Redirects_To_Add_Apprentice(
            ReservationsRouteModel routeModel,
            string addApprenticeUrl,
            SelectReservationViewModel viewModel,
            long expectedAccountId,
            long expectedAccountLegalEntityId,
            string expectedAccountPublicHashedId,
            string expectedAccountLegalEntityPublicHashedId,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            [Frozen]Mock<IExternalUrlHelper> urlHelper,
            [Frozen]Mock<IMediator> mediator,
            SelectReservationsController controller
        )
        {
            //Arrange
            var employer = new Employer
            {
                AccountLegalEntityPublicHashedId = expectedAccountLegalEntityPublicHashedId,
                AccountId = expectedAccountId,
                AccountPublicHashedId = expectedAccountPublicHashedId,
                AccountLegalEntityId = expectedAccountLegalEntityId
            };
            
            var reservationId = Guid.NewGuid();
            mediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = employer.AccountId,
                        AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = employer.AccountLegalEntityId,
                        AccountLegalEntityName = employer.AccountLegalEntityName,
                        AccountName = employer.AccountName,
                        CohortRef = viewModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });
            createReservationLevyResult.ReservationId = reservationId;
            urlHelper.Setup(x => x.GenerateAddApprenticeUrl(reservationId, employer.AccountLegalEntityPublicHashedId,
                    "", routeModel.UkPrn.Value, null, viewModel.CohortReference, routeModel.EmployerAccountId, false, viewModel.TransferSenderId, viewModel.EncodedPledgeApplicationId, viewModel.JourneyData))
                .Returns(addApprenticeUrl);
            mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

            //Act   
            var result = await controller.SelectReservation(routeModel, viewModel);


            //Assert
            Assert.IsNotNull(result);
            var actualRedirectResult = result as RedirectResult;
            Assert.IsNotNull(actualRedirectResult);
            Assert.AreEqual(actualRedirectResult.Url, addApprenticeUrl);

        }

        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployerAndReservationCreated_ThenRedirectsToAddAnApprentice(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            long expectedAccountId,
            long expectedTransferAccountId,
            long expectedAccountLegalEntityId,
            string expectedAccountPublicHashedId,
            string expectedAccountLegalEntityPublicHashedId,
            [Frozen]Mock<IEncodingService> mockEncodingService,
            [Frozen]Mock<IMediator> mockMediator,
            [Frozen]Mock<IExternalUrlHelper> urlHelper,
            string addApprenticeUrl,
            SelectReservationsController controller
        )
        {
            //Arrange
            var employer = new Employer
            {
                AccountLegalEntityPublicHashedId = expectedAccountLegalEntityPublicHashedId,
                AccountId = expectedAccountId,
                AccountPublicHashedId = expectedAccountPublicHashedId,
                AccountLegalEntityId = expectedAccountLegalEntityId
            };

            routeModel.AccountLegalEntityPublicHashedId = expectedAccountLegalEntityPublicHashedId;
           
            mockEncodingService.Setup(x => x.Encode(expectedAccountId, EncodingType.AccountId))
                .Returns(expectedAccountPublicHashedId);
            
            mockEncodingService.Setup(x => x.Decode(routeModel.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
                .Returns(expectedAccountLegalEntityId);
            
            createReservationLevyResult.ReservationId = Guid.NewGuid();

            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = employer.AccountId,
                        AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = employer.AccountLegalEntityId,
                        AccountLegalEntityName = employer.AccountLegalEntityName,
                        AccountName = employer.AccountName,
                        CohortRef = viewModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockEncodingService.Setup(x => x.Decode(viewModel.TransferSenderId, EncodingType.PublicAccountId))
                .Returns(expectedTransferAccountId);
            mockMediator.Setup(x => x.Send(It.Is<CreateReservationLevyEmployerCommand>(c=>
                    c.AccountId.Equals(expectedAccountId) &&
                    c.TransferSenderId.Equals(expectedTransferAccountId) &&
                    c.TransferSenderEmployerAccountId.Equals(viewModel.TransferSenderId) &&
                    c.AccountLegalEntityId.Equals(expectedAccountLegalEntityId)), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

            urlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(createReservationLevyResult.ReservationId,
                    routeModel.AccountLegalEntityPublicHashedId, "", routeModel.UkPrn.Value,
                    null, viewModel.CohortReference, routeModel.EmployerAccountId, 
                    false,viewModel.TransferSenderId, viewModel.EncodedPledgeApplicationId, viewModel.JourneyData))
                .Returns(addApprenticeUrl);
           
            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectResult;

            //Assert
            Assert.IsNotNull(result);
            result.Url.Should().Be(addApprenticeUrl);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Levy_Paying_Employer_With_No_Transfer_Id_And_No_CohortRef_Then_Redirects_To_Add_An_Apprentice_For_Empty_Cohort_Journey(
            ReservationsRouteModel routeModel,
            Employer employer,
            GetLegalEntitiesResponse employersResponse,
            long expectedAccountId,
            Guid? expectedUserId,
            string addAprrenticeUrl,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            [Frozen]Mock<IEncodingService> encodingService,
            [Frozen]Mock<IMediator> mediator,
            [Frozen]Mock<IExternalUrlHelper> urlHelper,
            [Frozen] Mock<IConfiguration> config,
            SelectReservationsController controller
        )
        {
            //Arrange
            config.Setup(x => x["AuthType"]).Returns("employer");
            employer.AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId;
            routeModel.UkPrn = null;
            viewModel.CohortReference = string.Empty;
            viewModel.TransferSenderId = string.Empty;
            viewModel.EncodedPledgeApplicationId = "";
            employer.AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId;
            employer.AccountId = expectedAccountId;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, expectedUserId.ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mediator
                .Setup(m => m.Send(
                    It.Is<GetLegalEntitiesQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            urlHelper.Setup(x => x.GenerateAddApprenticeUrl(It.IsAny<Guid>(),
                routeModel.AccountLegalEntityPublicHashedId, It.IsAny<string>(), viewModel.ProviderId, It.IsAny<DateTime?>(),
                viewModel.CohortReference, routeModel.EmployerAccountId, true,"", "",
                It.IsAny<string>())).Returns(addAprrenticeUrl);
            createReservationLevyResult.ReservationId = Guid.NewGuid();
            mediator.Setup(
                    x => x.Send(
                        It.Is<CreateReservationLevyEmployerCommand>(query => query.AccountId == employer.AccountId
                                                                    && query.UserId.Value == expectedUserId.Value), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

            //Act   
            var actual = await controller.SelectReservation(routeModel, viewModel);


            //Assert
            Assert.IsNotNull(actual);
            var actualResult = actual as RedirectResult;
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(addAprrenticeUrl, actualResult.Url);
        }

        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployerAndReservationCreated_With_No_Transfer_Id_ThenRedirectsToAddAnApprentice(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            long expectedAccountId,
            long expectedAccountLegalEntityId,
            string expectedAccountPublicHashedId,
            string expectedAccountLegalEntityPublicHashedId,
            [Frozen]Mock<IEncodingService> mockEncodingService,
            [Frozen]Mock<IMediator> mockMediator,
            [Frozen]Mock<IExternalUrlHelper> urlHelper,
            string addApprenticeUrl,
            SelectReservationsController controller
        )
        {
            //Arrange
            var employer = new Employer
            {
                AccountLegalEntityPublicHashedId = expectedAccountLegalEntityPublicHashedId,
                AccountId = expectedAccountId,
                AccountPublicHashedId = expectedAccountPublicHashedId,
                AccountLegalEntityId = expectedAccountLegalEntityId
            };

            viewModel.TransferSenderId = "";
            viewModel.EncodedPledgeApplicationId = "";
            routeModel.AccountLegalEntityPublicHashedId = expectedAccountLegalEntityPublicHashedId;
          

            mockEncodingService.Setup(x => x.Encode(expectedAccountId, EncodingType.AccountId))
                .Returns(expectedAccountPublicHashedId);
            
            mockEncodingService.Setup(x => x.Decode(routeModel.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
                .Returns(expectedAccountLegalEntityId);

            createReservationLevyResult.ReservationId = Guid.NewGuid();

            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = employer.AccountId,
                        AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = employer.AccountLegalEntityId,
                        AccountLegalEntityName = employer.AccountLegalEntityName,
                        AccountName = employer.AccountName,
                        CohortRef = routeModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockMediator.Setup(x => x.Send(It.Is<CreateReservationLevyEmployerCommand>(c =>
                    c.AccountId.Equals(expectedAccountId) &&
                    c.TransferSenderId.Equals(null) &&
                    c.AccountLegalEntityId.Equals(expectedAccountLegalEntityId)), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

            urlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(createReservationLevyResult.ReservationId, 
                    routeModel.AccountLegalEntityPublicHashedId,
                    "", 
                    routeModel.UkPrn.Value,
                    null, 
                    viewModel.CohortReference, 
                    routeModel.EmployerAccountId,
                    false,
                    "",
                    "",
                    It.IsAny<string>()))
                .Returns(addApprenticeUrl);
            
            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectResult;

            //Assert
            Assert.IsNotNull(result);
            result?.Url.Should().Be(addApprenticeUrl);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_No_Available_Reservations_For_A_Provider_Then_The_Cache_Is_Created_And_Redirected_To_Choose_Course(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = matchedEmployer.AccountId,
                        AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = matchedEmployer.AccountLegalEntityId,
                        AccountLegalEntityName = matchedEmployer.AccountLegalEntityName,
                        AccountName = matchedEmployer.AccountName,
                        CohortRef = viewModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult {Reservations = new List<Reservation>()});

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            //Assert
            mockMediator.Verify(x=>
                x.Send(It.Is<CacheReservationEmployerCommand>(
                        c=>
                           c.CohortRef.Equals(viewModel.CohortReference) &&
                           c.AccountId.Equals(matchedEmployer.AccountId) &&
                           c.AccountLegalEntityName.Equals(matchedEmployer.AccountLegalEntityName) &&
                           c.AccountLegalEntityPublicHashedId.Equals(matchedEmployer.AccountLegalEntityPublicHashedId) &&
                           c.AccountName.Equals(matchedEmployer.AccountName) &&
                           c.UkPrn.Equals(routeModel.UkPrn) &&
                           !c.Id.Equals(Guid.Empty) &&
                           c.AccountLegalEntityId.Equals(matchedEmployer.AccountLegalEntityId))
                    , It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.ProviderApprenticeshipTrainingRuleCheck,result.RouteName);
            Assert.AreNotEqual(Guid.Empty, routeModel.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_No_Available_Reservations_For_A_Employer_Then_The_Cache_Is_Created_And_Redirected_To_Choose_Course(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetLegalEntitiesResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            long expectedAccountId,
            [Frozen] Mock<IEncodingService> encodingService,
            SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(c=>c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c=>c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            //Assert
            mockMediator.Verify(x =>
                x.Send(It.Is<CacheReservationEmployerCommand>(
                        c =>
                            c.CohortRef.Equals(viewModel.CohortReference) &&
                            c.JourneyData.Equals(viewModel.JourneyData) &&
                            c.AccountId.Equals(expectedAccountId) &&
                            !c.IsEmptyCohortFromSelect &&
                            c.AccountLegalEntityName.Equals(matchedEmployer.AccountLegalEntityName) &&
                            c.AccountLegalEntityPublicHashedId.Equals(matchedEmployer.AccountLegalEntityPublicHashedId) &&
                            !c.Id.Equals(Guid.Empty) &&
                            c.AccountLegalEntityId.Equals(matchedEmployer.AccountLegalEntityId))
                    , It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.EmployerSelectCourseRuleCheck, result.RouteName);
            Assert.AreNotEqual(Guid.Empty, routeModel.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_No_Available_Reservations_For_An_Employer_And_I_Have_No_CohortRef_Then_Redirected_To_Choose_Course(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetLegalEntitiesResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            long expectedAccountId,
            uint providerId,
            [Frozen] Mock<IEncodingService> encodingService,
            SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
            viewModel.CohortReference = string.Empty;
            viewModel.ProviderId = providerId;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            //Assert
            mockMediator.Verify(x =>
                x.Send(It.Is<CacheReservationEmployerCommand>(
                        c =>
                            c.CohortRef.Equals(viewModel.CohortReference) &&
                            c.JourneyData.Equals(viewModel.JourneyData) &&
                            c.IsEmptyCohortFromSelect &&
                            c.UkPrn.Equals(providerId) &&
                            c.AccountId.Equals(expectedAccountId) &&
                            c.AccountLegalEntityName.Equals(matchedEmployer.AccountLegalEntityName) &&
                            c.AccountLegalEntityPublicHashedId.Equals(matchedEmployer.AccountLegalEntityPublicHashedId) &&
                            !c.Id.Equals(Guid.Empty) &&
                            c.AccountLegalEntityId.Equals(matchedEmployer.AccountLegalEntityId))
                    , It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.EmployerSelectCourseRuleCheck, result.RouteName);
            Assert.AreNotEqual(Guid.Empty, routeModel.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Reservation_Limit_Has_Been_Reached_The_Reservation_Limit_View_Is_Returned(
                ReservationsRouteModel routeModel,
                SelectReservationViewModel viewModel,
                GetTrustedEmployersResponse employersResponse,
                [Frozen] Mock<IMediator> mockMediator,
                long expectedAccountId,
                string cohortDetailsUrl,
                [Frozen] Mock<IEncodingService> encodingService,
                [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
                SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = matchedEmployer.AccountId,
                        AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = matchedEmployer.AccountLegalEntityId,
                        AccountLegalEntityName = matchedEmployer.AccountLegalEntityName,
                        AccountName = matchedEmployer.AccountName,
                        CohortRef = viewModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult {Reservations = new List<Reservation>()});
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ReservationLimitReachedException(expectedAccountId));
            mockUrlHelper
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId, 
                    viewModel.CohortReference, false, It.IsAny<string>(), routeModel.AccountLegalEntityPublicHashedId))
                .Returns(cohortDetailsUrl);

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be("ReservationLimitReached");
            result.Model.Should().Be(cohortDetailsUrl);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Is_A_Global_Restriction_In_Place_The_Funding_Paused_View_Is_Shown_For_Employer(
                ReservationsRouteModel routeModel,
                SelectReservationViewModel viewModel,
                GetLegalEntitiesResponse employersResponse,
                [Frozen] Mock<IMediator> mockMediator,
                long expectedAccountId,
                [Frozen] Mock<IEncodingService> encodingService,
                SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new GlobalReservationRuleException(expectedAccountId));
            
            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be("EmployerFundingPaused");
            
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Is_A_Global_Restriction_In_Place_The_Funding_Paused_View_Is_Shown_For_Provider(
                ReservationsRouteModel routeModel,
                SelectReservationViewModel viewModel,
                GetTrustedEmployersResponse employersResponse,
                GetAvailableReservationsResult reservationsResult,
                [Frozen] Mock<IMediator> mockMediator,
                long expectedAccountId,
                long expectedAccountLegalEntityId,
                [Frozen] Mock<IEncodingService> encodingService,
                SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = matchedEmployer.AccountId,
                        AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = matchedEmployer.AccountLegalEntityId,
                        AccountLegalEntityName = matchedEmployer.AccountLegalEntityName,
                        AccountName = matchedEmployer.AccountName,
                        CohortRef = viewModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new GlobalReservationRuleException(expectedAccountId));

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be("ProviderFundingPaused");
        }


        [Test, MoqAutoData]
        public async Task Then_If_The_Provider_Has_No_Create_Permission_The_NoPermissions_View_Is_Returned(
                ReservationsRouteModel routeModel,
                SelectReservationViewModel viewModel,
                GetTrustedEmployersResponse employersResponse,
                GetAvailableReservationsResult reservationsResult,
                [Frozen] Mock<IMediator> mockMediator,
                long expectedAccountId,
                long expectedAccountLegalEntityId,
                string cohortDetailsUrl,
                [Frozen] Mock<IEncodingService> encodingService,
                [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
                SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = matchedEmployer.AccountId,
                        AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = matchedEmployer.AccountLegalEntityId,
                        AccountLegalEntityName = matchedEmployer.AccountLegalEntityName,
                        AccountName = matchedEmployer.AccountName,
                        CohortRef = viewModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProviderNotAuthorisedException(expectedAccountId, routeModel.UkPrn.Value));
            mockUrlHelper
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId, 
                    viewModel.CohortReference, false, It.IsAny<string>(), routeModel.AccountLegalEntityPublicHashedId))
                .Returns(cohortDetailsUrl);

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be("NoPermissions");
            result.Model.Should().Be(cohortDetailsUrl);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Employer_Is_An_Owner_Non_Levy_And_Has_Not_Signed_An_Agreement_They_Are_Redirected_To_The_Agreement_Not_Signed_Page_For_Owner(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetLegalEntitiesResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            string cohortDetailsUrl,
            long expectedAccountId,
            long expectedAccountLegalEntityId,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IUserClaimsService> userClaimsService,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new EmployerAgreementNotSignedException(expectedAccountId,expectedAccountLegalEntityId));
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new GlobalReservationRuleException(expectedAccountId));
            userClaimsService.Setup(x => x.UserIsInRole(routeModel.EmployerAccountId, EmployerUserRole.Owner,
                controller.HttpContext.User.Claims)).Returns(true);
            mockUrlHelper
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId,
                    viewModel.CohortReference, false, viewModel.JourneyData, routeModel.AccountLegalEntityPublicHashedId))
                .Returns(cohortDetailsUrl);
            
            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            //Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be(RouteNames.EmployerOwnerSignAgreement);
            result.RouteValues["PreviousPage"].Should().Be(cohortDetailsUrl);
            result.RouteValues["IsFromSelect"].Should().Be(true);
            
        }
         [Test, MoqAutoData]
        public async Task Then_If_The_Employer_Is_An_Transactor_Non_Levy_And_Has_Not_Signed_An_Agreement_They_Are_Redirected_To_The_Agreement_Not_Signed_Page_For_Transactor(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetLegalEntitiesResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            string cohortDetailsUrl,
            long expectedAccountId,
            long expectedAccountLegalEntityId,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IUserClaimsService> userClaimsService,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
            var claim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, Guid.NewGuid().ToString());
            controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new EmployerAgreementNotSignedException(expectedAccountId,expectedAccountLegalEntityId));
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new GlobalReservationRuleException(expectedAccountId));
            userClaimsService.Setup(x => x.UserIsInRole(routeModel.EmployerAccountId, EmployerUserRole.Owner,
                controller.HttpContext.User.Claims)).Returns(false);
            mockUrlHelper
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId,
                    viewModel.CohortReference, false, viewModel.JourneyData, routeModel.AccountLegalEntityPublicHashedId))
                .Returns(cohortDetailsUrl);
            
            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            //Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be(RouteNames.EmployerTransactorSignAgreement);
            result.RouteValues["PreviousPage"].Should().Be(cohortDetailsUrl);
            result.RouteValues["IsFromSelect"].Should().Be(true);
            
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Provider_Is_Created_A_Reservation_For_A_Non_Levy_Employer_And_Has_Not_Signed_An_Agreement_They_Are_Redirected_To_The_Agreement_Not_Signed_Page(
            string cohortDetailsUrl,
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new EmployerAgreementNotSignedException(1,1));
            
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = matchedEmployer.AccountId,
                        AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = matchedEmployer.AccountLegalEntityId,
                        AccountLegalEntityName = matchedEmployer.AccountLegalEntityName,
                        AccountName = matchedEmployer.AccountName,
                        CohortRef = viewModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult {Reservations = new List<Reservation>()});
            mockUrlHelper
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId,
                    viewModel.CohortReference, false, viewModel.JourneyData, routeModel.AccountLegalEntityPublicHashedId))
                .Returns(cohortDetailsUrl);

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            //Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be(RouteNames.ProviderEmployerAgreementNotSigned);
            result.RouteValues["PreviousPage"].Should().Be(cohortDetailsUrl);
            result.RouteValues["IsFromSelect"].Should().Be(true);
        }

        [Test, MoqAutoData]
        public async Task And_The_Back_Link_Is_Select_Employer(
       string confirmEmployerUrl,
       ReservationsRouteModel routeModel,
       SelectReservationViewModel viewModel,
       Employer employer,
       GetAvailableReservationsResult reservationsResult,
       [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
       [Frozen] Mock<IMediator> mockMediator,
       [Frozen] Mock<IConfiguration> configuration,
       SelectReservationsController controller)
        {
            //Arrange
            routeModel.CohortReference = string.Empty;
            viewModel.CohortReference = string.Empty;
            configuration.Setup(x => x["AuthType"]).Returns("provider");
            routeModel.AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId;
            mockMediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((CreateReservationLevyEmployerResult)null);
            mockMediator.Setup(m =>
                    m.Send(It.IsAny<GetProviderCacheReservationCommandQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderCacheReservationCommandResponse
                {
                    Command = new CacheReservationEmployerCommand
                    {
                        Id = Guid.NewGuid(),
                        AccountId = employer.AccountId,
                        AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId,
                        AccountLegalEntityId = employer.AccountLegalEntityId,
                        AccountLegalEntityName = employer.AccountLegalEntityName,
                        AccountName = employer.AccountName,
                        CohortRef = routeModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);
            mockUrlHelper
                .Setup(helper => helper.GenerateConfirmEmployerUrl(routeModel.UkPrn.Value, routeModel.AccountLegalEntityPublicHashedId))
                .Returns(confirmEmployerUrl);

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            result.ViewName.Should().Be(ViewNames.Select);
            var actualModel = result.Model as SelectReservationViewModel;
            actualModel.Should().NotBeNull();
            actualModel.CohortReference.Should().Be(viewModel.CohortReference);
            actualModel.TransferSenderId.Should().Be(viewModel.TransferSenderId);
            actualModel.BackLink.Should().Be(confirmEmployerUrl);
            actualModel.AvailableReservations.Should().BeEquivalentTo(
                reservationsResult.Reservations
                    .Select(reservation => new AvailableReservationViewModel(reservation)));
        }
    }
}
