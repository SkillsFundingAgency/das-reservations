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
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;
using SFA.DAS.Reservations.Application.Providers.Queries;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    [TestFixture]
    public class WhenCallingChooseEmployer
    {
        [Test, MoqAutoData]
        public async Task Then_It_Calls_ProviderPermissions_Service_To_Get_Employers(
            ReservationsRouteModel routeModel,
            IEnumerable<AccountLegalEntity> expectedEmployers,
            GetLegalEntitiesResponse getLegalEntitiesResponse,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<ISessionStorageService<GetTrustedEmployersResponse>> sessionStorageService,
            ProviderReservationsController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetLegalEntitiesQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(getLegalEntitiesResponse);
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse{Employers = expectedEmployers});
        
            sessionStorageService.Setup(x => x.Get()).Returns((GetTrustedEmployersResponse)null);

            await controller.ChooseEmployer(routeModel);

            mockMediator.Verify(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(),  It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_Trusted_Employers(
            string accountLegalEntityPublicHashedId,
            ReservationsRouteModel routeModel,
            List<AccountLegalEntity> expectedEmployers,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<ISessionStorageService<GetTrustedEmployersResponse>> sessionStorageService,
            ProviderReservationsController controller)
        {
            routeModel.SearchTerm = null;
            routeModel.SortField = null;

            encodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityPublicHashedId);
            expectedEmployers = expectedEmployers.Select(arg => new AccountLegalEntity
            {
                AccountLegalEntityPublicHashedId = arg.AccountLegalEntityPublicHashedId,
                AccountId = arg.AccountId,
                IsLevy = false
            }).ToList();
            
            mockMediator
                .Setup(service => service.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse{Employers = expectedEmployers});

            sessionStorageService.Setup(x => x.Get()).Returns((GetTrustedEmployersResponse)null);

            var result = await controller.ChooseEmployer(routeModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ChooseEmployerViewModel>()
                .Subject;
            viewModel.Employers.Should()
                .BeEquivalentTo(expectedEmployers, options => options.Excluding(c=>c.AccountLegalEntityPublicHashedId));
            viewModel.Employers.All(c => c.AccountLegalEntityPublicHashedId.Equals(accountLegalEntityPublicHashedId))
                .Should().BeTrue();
        }

    }
}