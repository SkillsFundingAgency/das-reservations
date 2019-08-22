using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
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
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(validationException);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Exception_Then_Redirect_To_ProviderError(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            Employer employer,
            Exception exception,
            [Frozen] Mock<IMediator> mockMediator,
            SelectReservationsController controller)
        {
            routeModel.AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId;
            
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

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
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId,viewModel.CohortReference))
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
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            [Frozen]Mock<IMediator> mediator,
            SelectReservationsController controller
        )
        {
            //Arrange
            employer.AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId;

           
            var accountStatusResponse = new GetAccountReservationStatusResponse
            {
                CanAutoCreateReservations = true
            };

            createReservationLevyResult.ReservationId = Guid.NewGuid();
            
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
                        CohortRef = routeModel.CohortReference,
                        UkPrn = routeModel.UkPrn.Value
                    }
                });


            mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);
            
            //Act   
            await controller.SelectReservation(routeModel, viewModel);


            //Assert
            mediator.Verify(x =>
                x.Send(It.Is<CreateReservationLevyEmployerCommand>(query => query.AccountId == employer.AccountId), CancellationToken.None), Times.Once());

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
            var employersResponse = new GetTrustedEmployersResponse
            {
                Employers = new List<Employer>
                {
                    new Employer {AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId}
                }
            };

            createReservationLevyResult.ReservationId = Guid.NewGuid();

            mediator.Setup(x => x.Send(It.IsAny<GetTrustedEmployersQuery>(), CancellationToken.None))
                .ReturnsAsync(employersResponse);
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
            var employersResponse = new GetTrustedEmployersResponse
            {
                Employers = new List<Employer>
                {
                    new Employer {AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId}
                }
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
                    "", routeModel.UkPrn.Value, null, viewModel.CohortReference, routeModel.EmployerAccountId))
                .Returns(addApprenticeUrl);
            mediator.Setup(x => x.Send(It.IsAny<GetTrustedEmployersQuery>(), CancellationToken.None))
                .ReturnsAsync(employersResponse);
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
            GetAvailableReservationsResult reservationsResult,
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
                    null, viewModel.CohortReference, routeModel.EmployerAccountId))
                .Returns(addApprenticeUrl);
           
            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectResult;

            //Assert
            Assert.IsNotNull(result);
            result.Url.Should().Be(addApprenticeUrl);
        }


        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployerAndReservationCreated_With_No_Transfer_Id_ThenRedirectsToAddAnApprentice(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
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

            viewModel.TransferSenderId = null;
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

            
            mockMediator.Setup(x => x.Send(It.Is<GetAccountReservationStatusQuery>(c =>
                    c.AccountId.Equals(expectedAccountId) &&
                    c.HashedEmployerAccountId.Equals(expectedAccountPublicHashedId) &&
                    c.TransferSenderAccountId.Equals(string.Empty)), CancellationToken.None))
                .ReturnsAsync(new GetAccountReservationStatusResponse
                {
                    CanAutoCreateReservations = true
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
                    routeModel.EmployerAccountId))
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
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            long expectedAccountId,
            long expectedAccountLegalEntityId,
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
            Assert.AreEqual(RouteNames.ProviderApprenticeshipTraining,result.RouteName);
            Assert.AreNotEqual(Guid.Empty, routeModel.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_No_Available_Reservations_For_A_Employer_Then_The_Cache_Is_Created_And_Redirected_To_Choose_Course(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetLegalEntitiesResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            long expectedAccountId,
            long expectedAccountLegalEntityId,
            [Frozen] Mock<IEncodingService> encodingService,
            SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
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
                            c.AccountId.Equals(expectedAccountId) &&
                            c.AccountLegalEntityName.Equals(matchedEmployer.AccountLegalEntityName) &&
                            c.AccountLegalEntityPublicHashedId.Equals(matchedEmployer.AccountLegalEntityPublicHashedId) &&
                            !c.Id.Equals(Guid.Empty) &&
                            c.AccountLegalEntityId.Equals(matchedEmployer.AccountLegalEntityId))
                    , It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.EmployerSelectCourse, result.RouteName);
            Assert.AreNotEqual(Guid.Empty, routeModel.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Reservation_Limit_Has_Been_Reached_The_Reservation_Limit_View_Is_Returned(
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
                .ReturnsAsync(new GetAvailableReservationsResult {Reservations = new List<Reservation>()});
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ReservationLimitReachedException(expectedAccountId));
            mockUrlHelper
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId, viewModel.CohortReference))
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
                GetAvailableReservationsResult reservationsResult,
                [Frozen] Mock<IMediator> mockMediator,
                long expectedAccountId,
                long expectedAccountLegalEntityId,
                [Frozen] Mock<IEncodingService> encodingService,
                SelectReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
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
                .Setup(helper => helper.GenerateCohortDetailsUrl(routeModel.UkPrn, routeModel.EmployerAccountId, viewModel.CohortReference))
                .Returns(cohortDetailsUrl);

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be("NoPermissions");
            result.Model.Should().Be(cohortDetailsUrl);
        }
    }
}
