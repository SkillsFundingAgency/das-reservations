using System;
using System.Collections.Generic;
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
using SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Manage
{
    [TestFixture]
    public class WhenCallingGetProviderManage
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_List_Of_Reservations_From_Search(
            ReservationsRouteModel routeModel,
            ManageReservationsFilterModel filterModel,
            SearchReservationsResult searchResult,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<SearchReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResult);

            await controller.ProviderManage(routeModel, filterModel);

            mockMediator.Verify(mediator =>
                    mediator.Send(
                        It.Is<SearchReservationsQuery>(query => query.ProviderId == routeModel.UkPrn),//todo: yeah? or providerId?? :(
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_List_Of_Reservations_From_Mediator(
            [Frozen] ReservationsRouteModel routeModel,
            ManageReservationsFilterModel filterModel,
            SearchReservationsResult searchResult,
            string hashedId,
            string homeLink,
            [Frozen] ReservationsWebConfiguration config,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<SearchReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResult);
            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);
            externalUrlHelper
                .Setup(x => x.GenerateDashboardUrl(routeModel.EmployerAccountId)).Returns(homeLink);

            var expectedReservations = new List<ReservationViewModel>();
            expectedReservations.AddRange(searchResult.Reservations.Select(reservation =>
                new ReservationViewModel(reservation, config.ApprenticeUrl, routeModel.UkPrn)));
            
            var result = await controller.ProviderManage(routeModel, filterModel) as ViewResult;

            result.Should().NotBeNull();
            result.ViewName.Should().Be(ViewNames.ProviderManage);
            var viewModel = result.Model as ManageViewModel;
            viewModel.Should().NotBeNull();
            viewModel.BackLink.Should().Be(homeLink);
            viewModel.NumberOfRecordsFound.Should().Be(searchResult.NumberOfRecordsFound);
            viewModel.Reservations.Should().BeEquivalentTo(expectedReservations,
                options => options.ExcludingFields().Excluding(c=>c.ApprenticeUrl));
        }

        [Test, MoqAutoData]
        public async Task And_Reservation_From_This_Provider_Then_Is_Deletable(
            [Frozen] ReservationsRouteModel routeModel,
            ManageReservationsFilterModel filterModel,
            [ReservationsFromThisProvider] SearchReservationsResult searchResult,
            string hashedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<SearchReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResult);
            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);

            var result = await controller.ProviderManage(routeModel, filterModel) as ViewResult;

            var viewModel = result.Model as ManageViewModel;
            viewModel.Reservations
                .Select(model => model.CanProviderDeleteReservation)
                .Should().AllBeEquivalentTo(true);
        }

        [Test, MoqAutoData]
        public async Task And_Reservation_From_Different_Provider_Then_Not_Deletable(
            [Frozen] ReservationsRouteModel routeModel,
            ManageReservationsFilterModel filterModel,
            SearchReservationsResult searchResult,
            string hashedId,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<SearchReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResult);
            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);

            var result = await controller.ProviderManage(routeModel, filterModel) as ViewResult;

            var viewModel = result.Model as ManageViewModel;
            viewModel.Reservations
                .Select(model => model.CanProviderDeleteReservation)
                .Should().AllBeEquivalentTo(false);
        }

        [Test, MoqAutoData]
        public async Task Add_Apprentice_Url_UkPrn_Will_Be_Populated_From_RouteModel_Reservation(
            ReservationsRouteModel routeModel,
            ManageReservationsFilterModel filterModel,
            SearchReservationsResult searchResult,
            string hashedId,
            string expectedUrl,
            [Frozen] Mock<IEncodingService> mockEncodingService,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<IExternalUrlHelper> mockUrlHelper,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<SearchReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResult);

            mockEncodingService
                .Setup(service => service.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(hashedId);

            mockUrlHelper.Setup(h => h.GenerateAddApprenticeUrl(
                It.IsAny<Guid>(),
                hashedId,
                It.IsAny<string>(),
                routeModel.UkPrn,
                It.IsAny<DateTime>(),
                routeModel.CohortReference,
                routeModel.EmployerAccountId,
                false))
                .Returns(expectedUrl);
                
            var result = await controller.ProviderManage(routeModel, filterModel) as ViewResult;
            
            var viewModel = result?.Model as ManageViewModel;
            viewModel.Should().NotBeNull();

            viewModel.Reservations.Where(model => model.Status == ReservationStatusViewModel.Pending && !model.IsExpired)
                .All(c => c.ApprenticeUrl.Equals(expectedUrl)).Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task And_The_Provider_Has_No_TrustedEmployers_And_Is_A_Blank_Search_Query_Then_A_NoPermissions_View_Is_Returned(
            ReservationsRouteModel routeModel,
            ManageReservationsFilterModel filterModel,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            filterModel.SearchTerm = string.Empty;
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<SearchReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SearchReservationsResult{NumberOfRecordsFound = 0});

            var result = await controller.ProviderManage(routeModel, filterModel) as ViewResult;

            result.ViewName.Should().Be("NoPermissions");
        }

        [Test, MoqAutoData]
        public async Task Then_Filter_Params_Assigned_To_View_Model(
            ReservationsRouteModel routeModel,
            ManageReservationsFilterModel filterModel,
            SearchReservationsResult searchResult,
            [Frozen] Mock<IMediator> mockMediator,
            ManageReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(It.IsAny<SearchReservationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResult);

            var result = await controller.ProviderManage(routeModel, filterModel) as ViewResult;

            var viewModel = result?.Model as ManageViewModel;

            viewModel.FilterModel.Should().BeEquivalentTo(filterModel);
        }
    }
}