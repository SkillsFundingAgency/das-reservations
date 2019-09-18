using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    [TestFixture]
    public class WhenCallingGetSelectLegalEntity
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Legal_Entities(
            ReservationsRouteModel routeModel,
            long decodedAccountId,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            EmployerReservationsController controller)
        {
            routeModel.Id = null;
            mockEncodingService
                .Setup(service => service.Decode(routeModel.EmployerAccountId, EncodingType.AccountId))
                .Returns(decodedAccountId);
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);

            await controller.SelectLegalEntity(routeModel);

            mockMediator.Verify(mediator => mediator.Send(
                It.Is<GetLegalEntitiesQuery>(query => query.AccountId == decodedAccountId), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Only_One_Legal_Entity_Then_Adds_Legal_Entity_To_Reservation_And_Redirects_To_Course_Selection(
            ReservationsRouteModel routeModel,
            AccountLegalEntity accountLegalEntity,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            routeModel.Id = null;
            var getLegalEntitiesResponse = new GetLegalEntitiesResponse
            {
                AccountLegalEntities = new List<AccountLegalEntity> { accountLegalEntity }
            };
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);

            var result = await controller.SelectLegalEntity(routeModel) as RedirectToRouteResult;

            result.RouteName.Should().Be(RouteNames.EmployerSelectCourse);

            mockMediator.Verify(mediator => mediator.Send(
                    It.Is<CacheReservationEmployerCommand>(command =>
                        command.Id != Guid.Empty &&
                        command.AccountId == decodedAccountId &&
                        command.AccountLegalEntityId == accountLegalEntity.AccountLegalEntityId &&
                        command.AccountLegalEntityName == accountLegalEntity.AccountLegalEntityName &&
                        command.AccountLegalEntityPublicHashedId == accountLegalEntity.AccountLegalEntityPublicHashedId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Populated_View_Model(
            ReservationsRouteModel routeModel,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            routeModel.Id = null;

            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);

            var result = await controller.SelectLegalEntity(routeModel) as ViewResult;

            result.Should().NotBeNull();
            var viewModel = result?.Model.Should().BeOfType<SelectLegalEntityViewModel>().Subject;
            viewModel.Should().NotBeNull();
            viewModel?.RouteModel.Should().BeEquivalentTo(routeModel);
            viewModel?.LegalEntities.Should().BeEquivalentTo(getLegalEntitiesResponse.AccountLegalEntities, options => options.ExcludingMissingMembers());
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Is_A_Reservation_Created_The_Entity_Is_Taken_From_The_Cache(
            ReservationsRouteModel routeModel,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<GetLegalEntitiesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);

            var result = await controller.SelectLegalEntity(routeModel) as ViewResult;

            result.Should().NotBeNull();
            mockMediator.Verify(x => x.Send(It.Is<GetCachedReservationQuery>(c=>c.Id.Equals(routeModel.Id)), It.IsAny<CancellationToken>()), Times.Once);
            var viewModel = result?.Model.Should().BeOfType<SelectLegalEntityViewModel>().Subject;
            viewModel.Should().NotBeNull();
        }
    }
}