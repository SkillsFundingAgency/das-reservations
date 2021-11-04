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

        [Test]
        [MoqInlineAutoData(null, null, false, 6, null, null)]
        [MoqInlineAutoData("ltd", null, false, 3, null, null)]
        [MoqInlineAutoData("xxx", null, false, 0, null, null)]
        [MoqInlineAutoData("tesco", null, false, 1, null, null)]
        [MoqInlineAutoData(null, "AccountName", false, 6, "Aldi", "Tesco")]
        [MoqInlineAutoData("ltd", "AccountName", false, 3, "Asda", "Sainsbury's")]
        [MoqInlineAutoData("xxx", "AccountName", false, 0, null, null)]
        [MoqInlineAutoData("tesco", "AccountName", false, 1, "Tesco", "Tesco")]
        [MoqInlineAutoData(null, "AccountName", true, 6, "Tesco", "Aldi")]
        [MoqInlineAutoData("ltd", "AccountName", true, 3, "Sainsbury's", "Asda")]
        [MoqInlineAutoData("xxx", "AccountName", true, 0, null, null)]
        [MoqInlineAutoData("tesco", "AccountName", true, 1, "Tesco", "Tesco")]
        [MoqInlineAutoData(null, "AccountLegalEntityName", false, 6, "1 Lidl Plc", "6 Morrisons Ltd")]
        [MoqInlineAutoData("ltd", "AccountLegalEntityName", false, 3, "2 Sainsbury's Ltd", "6 Morrisons Ltd")]
        [MoqInlineAutoData("xxx", "AccountLegalEntityName", false, 0, null, null)]
        [MoqInlineAutoData("tesco", "AccountLegalEntityName", false, 1, "3 Tesco Limited", "3 Tesco Limited")]
        [MoqInlineAutoData(null, "AccountLegalEntityName", true, 6, "6 Morrisons Ltd", "1 Lidl Plc")]
        [MoqInlineAutoData("ltd", "AccountLegalEntityName", true, 3, "6 Morrisons Ltd", "2 Sainsbury's Ltd")]
        [MoqInlineAutoData("xxx", "AccountLegalEntityName", true, 0, null, null)]
        [MoqInlineAutoData("tesco", "AccountLegalEntityName", true, 1, "3 Tesco Limited", "3 Tesco Limited")]
        public async Task Then_It_Returns_The_Trusted_Employers(
            string searchTerm,
            string sortField,
            bool reverseDirection,
            int expectedResults,
            string firstItem,
            string lastItem,
            string accountLegalEntityPublicHashedId,
            ReservationsRouteModel routeModel,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] Mock<IMediator> mockMediator,
            [Frozen] Mock<ISessionStorageService<GetTrustedEmployersResponse>> sessionStorageService,
            ProviderReservationsController controller)
        {
            var ale = AccountLegalEntities().ToList();

            routeModel.SearchTerm = searchTerm;
            routeModel.SortField = sortField;
            routeModel.ReverseSort = reverseDirection;

            encodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.PublicAccountLegalEntityId))
                .Returns(accountLegalEntityPublicHashedId);

            mockMediator
                .Setup(service => service.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse{Employers = ale});

            sessionStorageService.Setup(x => x.Get()).Returns((GetTrustedEmployersResponse)null);

            var result = await controller.ChooseEmployer(routeModel);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ChooseEmployerViewModel>()
                .Subject;

            viewModel.Employers.Count().Should().Be(expectedResults);
            
            viewModel.Employers.All(c => c.AccountLegalEntityPublicHashedId.Equals(accountLegalEntityPublicHashedId)).Should().BeTrue();

            if (viewModel.Employers.Any() && !string.IsNullOrWhiteSpace(sortField))
            {
                viewModel.Employers.First().GetType().GetProperty(sortField).GetValue(viewModel.Employers.First(), null).ToString().Should().Be(firstItem);
                viewModel.Employers.Last().GetType().GetProperty(sortField).GetValue(viewModel.Employers.Last(), null).ToString().Should().Be(lastItem);
            }
        }

        private IEnumerable<AccountLegalEntity> AccountLegalEntities()
        {
            return new List<AccountLegalEntity>
            {
                new AccountLegalEntity
                {
                    AccountId = 1,
                    AccountName = "Tesco",
                    AccountLegalEntityId = 1,
                    AccountLegalEntityName = "3 Tesco Limited"
                },
                new AccountLegalEntity
                {
                    AccountId = 2,
                    AccountName = "Asda",
                    AccountLegalEntityId = 2,
                    AccountLegalEntityName = "5 Asda Ltd"
                },
                new AccountLegalEntity
                {
                    AccountId = 3,
                    AccountName = "Lidl",
                    AccountLegalEntityId = 3,
                    AccountLegalEntityName = "1 Lidl Plc"
                },
                new AccountLegalEntity
                {
                    AccountId = 4,
                    AccountName = "Morrisons",
                    AccountLegalEntityId = 4,
                    AccountLegalEntityName = "6 Morrisons Ltd"
                },
                new AccountLegalEntity
                {
                    AccountId = 5,
                    AccountName = "Sainsbury's",
                    AccountLegalEntityId = 5,
                    AccountLegalEntityName = "2 Sainsbury's Ltd"
                },
                new AccountLegalEntity
                {
                    AccountId = 6,
                    AccountName = "Aldi",
                    AccountLegalEntityId = 6,
                    AccountLegalEntityName = "4 Aldi"
                },
            };
        }
    }
}