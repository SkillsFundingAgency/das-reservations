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
        public async Task And_Has_Ukprn_Then_Gets_Employer_From_Trusted_Employers(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            await controller.SelectReservation(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetTrustedEmployersQuery>(query => query.UkPrn == routeModel.UkPrn), 
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_ValidationException_Then_Redirect_To_ProviderError(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            ValidationException validationException,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
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
            GetTrustedEmployersResponse employersResponse,
            Exception exception,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Employer_Not_Found_Then_Get_Reservations_From_Legal_Entity(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller, 
            [Frozen]AccountLegalEntity accountLegalEntity, 
            GetAvailableReservationsResult reservationsResult)
        {
            routeModel.AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId;

            accountLegalEntity.AccountId = "23";

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);

            mockMediator.Setup(mediator =>
                    mediator.Send(
                        It.IsAny<GetAccountLegalEntityQuery>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountLegalEntityResult{LegalEntity = accountLegalEntity});

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            await controller.SelectReservation(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetAvailableReservationsQuery>(q => q.AccountId.Equals(long.Parse(accountLegalEntity.AccountId))),
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Employer_And_Legal_Entity_Not_Found_Then_Redirect_To_Error(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller, 
            [Frozen]AccountLegalEntity accountLegalEntity, 
            GetAvailableReservationsResult reservationsResult)
        {
            routeModel.AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId;

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);

            mockMediator.Setup(mediator =>
                    mediator.Send(
                        It.IsAny<GetAccountLegalEntityQuery>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountLegalEntityResult());

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
            GetTrustedEmployersResponse employersResponse,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller, 
            [Frozen]AccountLegalEntity accountLegalEntity, 
            GetAvailableReservationsResult reservationsResult)
        {
            routeModel.AccountLegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId;

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);

            mockMediator.Setup(mediator =>
                    mediator.Send(
                        It.IsAny<GetAccountLegalEntityQuery>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountLegalEntityResult{LegalEntity = accountLegalEntity});

            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

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
            ReservationsController controller)
        {
            routeModel.UkPrn = null;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetLegalEntitiesQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);

            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.Error500);
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_Then_Gets_Reservations_For_Employer_Account(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
           
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            await controller.SelectReservation(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetAvailableReservationsQuery>(query => query.AccountId == matchedEmployer.AccountId), 
                It.IsAny<CancellationToken>()));
        }

        [Test, MoqAutoData]
        public async Task And_Has_Ukprn_And_Has_Reservations_Then_Shows_ProviderSelect_View(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetTrustedEmployersResponse employersResponse,
            GetAvailableReservationsResult reservationsResult,
            [Frozen] Mock<IMediator> mockMediator,
            ReservationsController controller)
        {
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
           
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAvailableReservationsQuery>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservationsResult);

            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            result.ViewName.Should().Be(ViewNames.ProviderSelect);
            var actualModel = result.Model as SelectReservationViewModel;
            actualModel.Should().NotBeNull();
            actualModel.CohortReference.Should().Be(viewModel.CohortReference);
            actualModel.TransferSenderId.Should().Be(viewModel.TransferSenderId);
            actualModel.AvailableReservations.Should().BeEquivalentTo(
                reservationsResult.Reservations
                    .Select(reservation => new AvailableReservationViewModel(reservation)));
        }
        
        [Test, MoqAutoData]
        public async Task ThenChecksEmployerLevyStatus(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            [Frozen] GetTrustedEmployersResponse employersResponse,
            Employer employer,
            GetAvailableReservationsResult reservationsResult,
            GetAccountReservationStatusResponse accountStatusResponse,
            [Frozen]Mock<IMediator> mediator,
            ReservationsController controller
            )
        {
            //Arrange
            routeModel.UkPrn = 2442;
            routeModel.AccountLegalEntityPublicHashedId = employer.AccountLegalEntityPublicHashedId;
            accountStatusResponse.CanAutoCreateReservations = true;
            employersResponse.Employers = new List<Employer> {employer};
            mediator.Setup(x => x.Send(It.IsAny<GetTrustedEmployersQuery>(), CancellationToken.None))
                .ReturnsAsync(employersResponse);
            mediator.Setup(x => x.Send(It.IsAny<GetAccountReservationStatusQuery>(), CancellationToken.None))
                .ReturnsAsync(accountStatusResponse);

            //Act
            await controller.SelectReservation(routeModel, viewModel);

            //Assert
            mediator.Verify(x =>
                x.Send(It.Is<GetAccountReservationStatusQuery>(query => query.AccountId == employer.AccountId),CancellationToken.None),Times.Once());
        }

        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployer_ThenCreatesReservationInTheBackground(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            [Frozen]Mock<IMediator> mediator,
            ReservationsController controller
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
            var accountStatusResponse = new GetAccountReservationStatusResponse
            {
                CanAutoCreateReservations = true
            };

            createReservationLevyResult.ReservationId = Guid.NewGuid();
            
            mediator.Setup(x => x.Send(It.IsAny<GetTrustedEmployersQuery>(), CancellationToken.None))
                .ReturnsAsync(employersResponse);
            mediator.Setup(x => x.Send(It.IsAny<GetAccountReservationStatusQuery>(), CancellationToken.None))
                .ReturnsAsync(accountStatusResponse);
            mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);
            
            //Act   
            await controller.SelectReservation(routeModel, viewModel);


            //Assert
            mediator.Verify(x =>
                x.Send(It.Is<CreateReservationLevyEmployerCommand>(query => query.AccountId == employersResponse.Employers.First().AccountId), CancellationToken.None), Times.Once());

        }

        [Test, MoqAutoData]
        public async Task And_Has_Transfer_Sender_Which_Is_Not_Valid_Goes_To_Error(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            [Frozen]Mock<IMediator> mediator,
            ReservationsController controller
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
            mediator.Setup(x => x.Send(It.IsAny<GetAccountReservationStatusQuery>(), CancellationToken.None))
                .ThrowsAsync(new TransferSendNotAllowedException(1,"1"));
            mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

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
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            [Frozen]Mock<IMediator> mediator,
            ReservationsController controller
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
            mediator.Setup(x => x.Send(It.IsAny<GetAccountReservationStatusQuery>(), CancellationToken.None))
                .ThrowsAsync(new TransferSendNotAllowedException(1, "1"));
            mediator.Setup(x => x.Send(It.IsAny<CreateReservationLevyEmployerCommand>(), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

            //Act   
            var result = await controller.SelectReservation(routeModel, viewModel);


            //Assert
            Assert.IsNotNull(result);
            var actualRedirectResult = result as RedirectToRouteResult;
            Assert.IsNotNull(actualRedirectResult);
            Assert.AreEqual(actualRedirectResult.RouteName, RouteNames.Error500);

        }

        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployerAndReservationCreated_ThenRedirectsToAddAnApprentice(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            long expectedAccountId,
            long? expectedTransferAccountId,
            long expectedAccountLegalEntityId,
            string expectedAccountPublicHashedId,
            string expectedAccountLegalEntityPublicHashedId,
            [Frozen]Mock<IEncodingService> mockEncodingService,
            [Frozen]Mock<IMediator> mockMediator,
            [Frozen]Mock<IExternalUrlHelper> urlHelper,
            string addApprenticeUrl,
            ReservationsController controller
        )
        {
            //Arrange
            routeModel.AccountLegalEntityPublicHashedId = expectedAccountLegalEntityPublicHashedId;
            var employersResponse = new GetTrustedEmployersResponse
            {
                Employers = new List<Employer>
                {
                    new Employer
                    {
                        AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                        AccountId = expectedAccountId,
                        AccountPublicHashedId = expectedAccountPublicHashedId,
                        AccountLegalEntityId = expectedAccountLegalEntityId
                    }
                }
            };

            mockEncodingService.Setup(x => x.Decode(viewModel.TransferSenderId, EncodingType.PublicAccountId))
                .Returns(expectedTransferAccountId.Value);
            mockEncodingService.Setup(x => x.Encode(expectedAccountId, EncodingType.AccountId))
                .Returns(expectedAccountPublicHashedId);
            mockEncodingService.Setup(x => x.Decode(routeModel.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
                .Returns(expectedAccountLegalEntityId);
            
            createReservationLevyResult.ReservationId = Guid.NewGuid();

            mockMediator.Setup(x => x.Send(It.Is<GetTrustedEmployersQuery>(c=>c.UkPrn.Equals(routeModel.UkPrn.Value)), CancellationToken.None))
                .ReturnsAsync(employersResponse);
            mockMediator.Setup(x => x.Send(It.Is<GetAccountReservationStatusQuery>(c=>
                    c.AccountId.Equals(expectedAccountId) &&
                    c.HashedEmployerAccountId.Equals(expectedAccountPublicHashedId) &&
                    c.TransferSenderAccountId.Equals(viewModel.TransferSenderId)), CancellationToken.None))
                .ReturnsAsync(new GetAccountReservationStatusResponse
                {
                    CanAutoCreateReservations = true
                });
            mockMediator.Setup(x => x.Send(It.Is<CreateReservationLevyEmployerCommand>(c=>
                    c.AccountId.Equals(expectedAccountId) &&
                    c.TransferSenderId.Equals(expectedTransferAccountId) &&
                    c.AccountLegalEntityId.Equals(expectedAccountLegalEntityId)), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

            urlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Id == routeModel.UkPrn.ToString() &&
                        parameters.Controller == $"unapproved/{viewModel.CohortReference}" &&
                        parameters.Action == "apprentices/add" &&
                        parameters.QueryString == $"?reservationId={createReservationLevyResult.ReservationId}" +
                        $"&employerAccountLegalEntityPublicHashedId={routeModel.AccountLegalEntityPublicHashedId}" 
                        )))
                .Returns(addApprenticeUrl);

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as RedirectResult;

            //Assert
            Assert.IsNotNull(result);
            result?.Url.Should().Be(addApprenticeUrl);
        }


        [Test, MoqAutoData]
        public async Task AndHasLevyPayingEmployerAndReservationCreated_With_No_Transfer_Id_ThenRedirectsToAddAnApprentice(
            ReservationsRouteModel routeModel,
            SelectReservationViewModel viewModel,
            GetAvailableReservationsResult reservationsResult,
            CreateReservationLevyEmployerResult createReservationLevyResult,
            long expectedAccountId,
            long? expectedTransferAccountId,
            long expectedAccountLegalEntityId,
            string expectedAccountPublicHashedId,
            string expectedAccountLegalEntityPublicHashedId,
            [Frozen]Mock<IEncodingService> mockEncodingService,
            [Frozen]Mock<IMediator> mockMediator,
            [Frozen]Mock<IExternalUrlHelper> urlHelper,
            string addApprenticeUrl,
            ReservationsController controller
        )
        {
            //Arrange
            viewModel.TransferSenderId = null;
            expectedTransferAccountId = null;
            routeModel.AccountLegalEntityPublicHashedId = expectedAccountLegalEntityPublicHashedId;
            var employersResponse = new GetTrustedEmployersResponse
            {
                Employers = new List<Employer>
                {
                    new Employer
                    {
                        AccountLegalEntityPublicHashedId = routeModel.AccountLegalEntityPublicHashedId,
                        AccountId = expectedAccountId,
                        AccountPublicHashedId = expectedAccountPublicHashedId,
                        AccountLegalEntityId = expectedAccountLegalEntityId
                    }
                }
            };

            mockEncodingService.Setup(x => x.Encode(expectedAccountId, EncodingType.AccountId))
                .Returns(expectedAccountPublicHashedId);
            mockEncodingService.Setup(x => x.Decode(routeModel.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
                .Returns(expectedAccountLegalEntityId);

            createReservationLevyResult.ReservationId = Guid.NewGuid();

            mockMediator.Setup(x => x.Send(It.Is<GetTrustedEmployersQuery>(c => c.UkPrn.Equals(routeModel.UkPrn.Value)), CancellationToken.None))
                .ReturnsAsync(employersResponse);
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
                    c.TransferSenderId.Equals(expectedTransferAccountId) &&
                    c.AccountLegalEntityId.Equals(expectedAccountLegalEntityId)), CancellationToken.None))
                .ReturnsAsync(createReservationLevyResult);

            urlHelper
                .Setup(helper => helper.GenerateAddApprenticeUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Id == routeModel.UkPrn.ToString() &&
                        parameters.Controller == $"unapproved/{viewModel.CohortReference}" &&
                        parameters.Action == "apprentices/add" &&
                        parameters.QueryString == $"?reservationId={createReservationLevyResult.ReservationId}" +
                        $"&employerAccountLegalEntityPublicHashedId={routeModel.AccountLegalEntityPublicHashedId}"
                        )))
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
            ReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
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
            ReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            routeModel.UkPrn = null;
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.AccountLegalEntities.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
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
            Assert.AreEqual(RouteNames.EmployerApprenticeshipTraining, result.RouteName);
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
                ReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult {Reservations = new List<Reservation>()});
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ReservationLimitReachedException(expectedAccountId));
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Id == routeModel.UkPrn.ToString() &&
                        parameters.Controller == $"apprentices/{viewModel.CohortReference}" &&
                        parameters.Action == "details")))
                .Returns(cohortDetailsUrl);

            //Act
            var result = await controller.SelectReservation(routeModel, viewModel) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be("ReservationLimitReached");
            result.Model.Should().Be(cohortDetailsUrl);
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
                ReservationsController controller)
        {
            //Arrange
            routeModel.Id = Guid.Empty;
            encodingService.Setup(x => x.Decode(routeModel.EmployerAccountId, EncodingType.AccountId)).Returns(expectedAccountId);
            var matchedEmployer = employersResponse.Employers.First();
            routeModel.AccountLegalEntityPublicHashedId = matchedEmployer.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(employersResponse);
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetAvailableReservationsQuery>(c => c.AccountId.Equals(expectedAccountId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableReservationsResult { Reservations = new List<Reservation>() });
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProviderNotAuthorisedException(expectedAccountId, routeModel.UkPrn.Value));
            mockUrlHelper
                .Setup(helper => helper.GenerateUrl(
                    It.Is<UrlParameters>(parameters =>
                        parameters.Id == routeModel.UkPrn.ToString() &&
                        parameters.Controller == $"apprentices/{viewModel.CohortReference}" &&
                        parameters.Action == "details")))
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
