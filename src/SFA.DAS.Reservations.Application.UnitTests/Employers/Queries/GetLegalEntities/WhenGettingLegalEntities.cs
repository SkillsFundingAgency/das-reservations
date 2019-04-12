using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntities;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries.GetLegalEntities
{
    [TestFixture]
    public class WhenGettingLegalEntities
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Legal_Entities_For_Account(
            GetLegalEntitiesQuery query,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            GetLegalEntitiesQueryHandler handler)
        {
            await handler.Handle(query, CancellationToken.None);

            mockAccountApiClient.Verify(client => client.GetLegalEntitiesConnectedToAccount(query.AccountId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Legal_Entities_Details(
            GetLegalEntitiesQuery query,
            List<ResourceViewModel> resourceViewModels,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            GetLegalEntitiesQueryHandler handler)
        {
            mockAccountApiClient
                .Setup(client => client.GetLegalEntitiesConnectedToAccount(It.IsAny<string>()))
                .ReturnsAsync(resourceViewModels);

            await handler.Handle(query, CancellationToken.None);

            resourceViewModels.ForEach(model => 
                mockAccountApiClient.Verify(client => client.GetResource<LegalEntityViewModel>(model.Href), Times.Once));
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Legal_Entities(
            GetLegalEntitiesQuery query,
            List<ResourceViewModel> resourceViewModels,
            LegalEntityViewModel legalEntityViewModel,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            GetLegalEntitiesQueryHandler handler)
        {
            mockAccountApiClient
                .Setup(client => client.GetLegalEntitiesConnectedToAccount(It.IsAny<string>()))
                .ReturnsAsync(resourceViewModels);
            mockAccountApiClient
                .Setup(client => client.GetResource<LegalEntityViewModel>(It.IsAny<string>()))
                .ReturnsAsync(legalEntityViewModel);
            
            var result = await handler.Handle(query, CancellationToken.None);

            result.LegalEntityViewModels.Count().Should().Be(resourceViewModels.Count);
            result.LegalEntityViewModels.First().Should().BeEquivalentTo(legalEntityViewModel);
        }
    }
}