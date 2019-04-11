using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
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
    }
}