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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    [TestFixture]
    public class WhenCallingPostSelectLegalEntity
    {
        [Test, MoqAutoData]
        public async Task And_Model_Invalid_Then_Shows_View_Again(
            ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
             [NoAutoProperties] EmployerReservationsController controller)
        {
            controller.ModelState.AddModelError("test", "test");

            var result = await controller.PostSelectLegalEntity(routeModel, viewModel) as ViewResult;

            result.Should().NotBeNull();
            result.ViewName.Should().Be("SelectLegalEntity");

            mockMediator.Verify(mediator => mediator.Send(
                    It.IsAny<CacheReservationEmployerCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_Caches_New_Reservation(
            ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            long decodedAccountId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
             [NoAutoProperties] EmployerReservationsController controller)
        {
            var firstLegalEntity = getLegalEntitiesResponse.AccountLegalEntities.First();
            firstLegalEntity.AgreementSigned = true;
            viewModel.LegalEntity = firstLegalEntity.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(query => query.AccountId == decodedAccountId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(routeModel.EmployerAccountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            await controller.PostSelectLegalEntity(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                    It.Is<CacheReservationEmployerCommand>(command =>
                        command.Id != Guid.Empty &&
                        command.AccountId == firstLegalEntity.AccountId &&
                        command.AccountLegalEntityId == firstLegalEntity.AccountLegalEntityId &&
                        command.AccountLegalEntityName == firstLegalEntity.AccountLegalEntityName &&
                        command.AccountLegalEntityPublicHashedId == firstLegalEntity.AccountLegalEntityPublicHashedId &&
                        !command.EmployerHasSingleLegalEntity),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Updates_Existing_Reservation_If_Supplied(ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            long decodedAccountId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
             [NoAutoProperties] EmployerReservationsController controller)
        {
            var firstLegalEntity = getLegalEntitiesResponse.AccountLegalEntities.First();
            firstLegalEntity.AgreementSigned = true;
            viewModel.LegalEntity = firstLegalEntity.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(query => query.AccountId == decodedAccountId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockEncodingService
                .Setup(service => service.Decode(routeModel.EmployerAccountId, EncodingType.AccountId))
                .Returns(decodedAccountId);

            await controller.PostSelectLegalEntity(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                    It.Is<CacheReservationEmployerCommand>(command =>
                        command.Id == routeModel.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Redirects_To_Select_Course(
            ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            var firstLegalEntity = getLegalEntitiesResponse.AccountLegalEntities.First();
            firstLegalEntity.AgreementSigned = true;
            viewModel.LegalEntity = firstLegalEntity.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetLegalEntitiesQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);

            var result = await controller.PostSelectLegalEntity(routeModel, viewModel) as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteName.Should().Be(RouteNames.EmployerSelectCourse);
        }

        [Test, MoqAutoData]
        public async Task And_ValidationException_Then_Redirects_To_SelectLegalEntity(
            ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IMediator> mockMediator,
             [NoAutoProperties] EmployerReservationsController controller)
        {
            var firstLegalEntity = getLegalEntitiesResponse.AccountLegalEntities.First();
            firstLegalEntity.AgreementSigned = true;
            viewModel.LegalEntity = firstLegalEntity.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetLegalEntitiesQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockMediator.Setup(x => x.Send(It.IsAny<CacheReservationEmployerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException(
                    new ValidationResult("Failed",
                        new List<string> {"AccountId| Account reservation limit has been reached."}), null, null));

            var actual = await controller.PostSelectLegalEntity(routeModel, viewModel);

            actual.Should().NotBeNull();
            var actualViewResult = actual as ViewResult;
            actualViewResult.Should().NotBeNull();
            actualViewResult?.ViewName.Should().Be("SelectLegalEntity");
            controller.ModelState.IsValid.Should().BeFalse();
            controller.ModelState.Should().Contain(pair => pair.Key == "AccountId");
        }
        
        [Test, MoqAutoData]
        public async Task And_Global_Rule_Exists_Then_Shows_Funding_Paused_Page(
            ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
            [NoAutoProperties] EmployerReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new GlobalReservationRuleException(10));

            var result = await controller.PostSelectLegalEntity(routeModel, viewModel) as ViewResult;

            Assert.AreEqual("EmployerFundingPaused", result?.ViewName);
        }

        [Test, MoqAutoData]
        public async Task And_Reservation_Limit_Has_Been_Exceeded_Then_Shows_Reservation_Limit_Reached_Page(
            ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            [Frozen] Mock<IMediator> mockMediator,
             [NoAutoProperties] EmployerReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ReservationLimitReachedException(10));

            var result = await controller.PostSelectLegalEntity(routeModel, viewModel) as ViewResult;

            Assert.AreEqual("ReservationLimitReached", result?.ViewName);
        }

        [Test, MoqAutoData]
        public async Task
            And_User_Has_Owner_Role_And_Chosen_Legal_Entity_Has_Not_Signed_Agreement_Then_Redirect_To_Owner_Sign_Route(
                ReservationsRouteModel routeModel,
                ConfirmLegalEntityViewModel viewModel,
                GetLegalEntitiesResponse getLegalEntitiesResponse,
                [Frozen] Mock<IMediator> mockMediator,
                [Frozen] Mock<IUserClaimsService> mockClaimsService,
                [NoAutoProperties] EmployerReservationsController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("X1", "2") }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };
            var firstLegalEntity = getLegalEntitiesResponse.AccountLegalEntities.First();
            firstLegalEntity.AgreementSigned = false;
            viewModel.LegalEntity = firstLegalEntity.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetLegalEntitiesQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockClaimsService
                .Setup(service => service.UserIsInRole(
                    routeModel.EmployerAccountId,
                    EmployerUserRole.Owner,
                    It.IsAny<IEnumerable<Claim>>()))
                .Returns(true);

            var result = await controller.PostSelectLegalEntity(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.EmployerOwnerSignAgreement);
            result.RouteValues[nameof(ReservationsRouteModel.PreviousPage)].Should().Be(RouteNames.EmployerSelectLegalEntity);
        }

        [Test, MoqAutoData]
        public async Task
            And_User_Has_Transactor_Role_And_Chosen_Legal_Entity_Has_Not_Signed_Agreement_Then_Redirect_To_Transactor_Sign_Route(
                ReservationsRouteModel routeModel,
                ConfirmLegalEntityViewModel viewModel,
                GetLegalEntitiesResponse getLegalEntitiesResponse,
                [Frozen] Mock<IMediator> mockMediator,
                [Frozen] Mock<IUserClaimsService> mockClaimsService,
                [NoAutoProperties] EmployerReservationsController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("X1", "2") }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                { User = user }
            };
            var firstLegalEntity = getLegalEntitiesResponse.AccountLegalEntities.First();
            firstLegalEntity.AgreementSigned = false;
            viewModel.LegalEntity = firstLegalEntity.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetLegalEntitiesQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockClaimsService
                .Setup(service => service.UserIsInRole(
                    routeModel.EmployerAccountId,
                    EmployerUserRole.Owner,
                    It.IsAny<IEnumerable<Claim>>()))
                .Returns(false);

            var result = await controller.PostSelectLegalEntity(routeModel, viewModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.EmployerTransactorSignAgreement);
            result.RouteValues[nameof(ReservationsRouteModel.PreviousPage)].Should().Be(RouteNames.EmployerSelectLegalEntity);
        }
    }
}
