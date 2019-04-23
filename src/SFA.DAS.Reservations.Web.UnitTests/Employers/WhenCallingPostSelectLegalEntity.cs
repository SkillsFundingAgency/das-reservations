using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;

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
            EmployerReservationsController controller)
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
            [Frozen] Mock<IHashingService> mockHashingService,
            EmployerReservationsController controller)
        {
            var firstLegalEntity = getLegalEntitiesResponse.AccountLegalEntities.First();
            viewModel.LegalEntity = firstLegalEntity.AccountLegalEntityPublicHashedId;
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetLegalEntitiesQuery>(query => query.AccountId == routeModel.EmployerAccountId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockHashingService
                .Setup(service => service.DecodeValue(routeModel.EmployerAccountId))
                .Returns(decodedAccountId);

            await controller.PostSelectLegalEntity(routeModel, viewModel);

            mockMediator.Verify(mediator => mediator.Send(
                    It.Is<CacheReservationEmployerCommand>(command => 
                        command.Id != Guid.Empty &&
                        command.AccountId == decodedAccountId &&
                        command.AccountLegalEntityId == firstLegalEntity.AccountLegalEntityId &&
                        command.AccountLegalEntityName == firstLegalEntity.Name &&
                        command.AccountLegalEntityPublicHashedId == firstLegalEntity.AccountLegalEntityPublicHashedId), 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Redirects_To_Select_Course(
            ReservationsRouteModel routeModel,
            ConfirmLegalEntityViewModel viewModel,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            long decodedAccountId,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IHashingService> mockHashingService,
            EmployerReservationsController controller)
        {
            var firstLegalEntity = getLegalEntitiesResponse.AccountLegalEntities.First();
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
    }
}