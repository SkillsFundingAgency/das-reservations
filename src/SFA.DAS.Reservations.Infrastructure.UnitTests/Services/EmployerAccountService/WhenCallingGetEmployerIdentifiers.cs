using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Domain.Authentication;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.EmployerAccountService
{
    public class WhenCallingGetEmployerIdentifiers
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Accounts_From_AccountApi(
            string userId,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            Infrastructure.Services.EmployerAccountService employerAccountService)
        {
            await employerAccountService.GetEmployerIdentifiersAsync(userId);

            mockAccountApiClient.Verify(client => client.GetUserAccounts(userId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Maps_Accounts_To_EmployerIdentifiers(
            string userId,
            List<AccountDetailViewModel> accountDetailViewModels,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            Infrastructure.Services.EmployerAccountService employerAccountService)
        {
            mockAccountApiClient
                .Setup(client => client.GetUserAccounts(It.IsAny<string>()))
                .ReturnsAsync(accountDetailViewModels);

            var result = await employerAccountService.GetEmployerIdentifiersAsync(userId);

            result.Should().BeEquivalentTo(accountDetailViewModels.Select(model => new EmployerIdentifier
            {
                AccountId = model.HashedAccountId,
                EmployerName = model.DasAccountName,
                LegalEntityResources = model.LegalEntities
            }));
        }
    }
}