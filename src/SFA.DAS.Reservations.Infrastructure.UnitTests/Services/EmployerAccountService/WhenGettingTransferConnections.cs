using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.EmployerAccountService
{
    public class WhenGettingTransferConnections
    {
        [SetUp]
        public void Arrange()
        {

        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Transfer_Accounts_From_AccountApi_And_Maps_To_Model(
            string accountId,
         List<TransferConnectionViewModel> apiResponse,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            Infrastructure.Services.EmployerAccountService employerAccountService)
        {
            mockAccountApiClient
                .Setup(client => client.GetTransferConnections(accountId))
                .ReturnsAsync(apiResponse);
                
            var actual = await employerAccountService.GetTransferConnections(accountId);

            mockAccountApiClient.Verify(client => client.GetTransferConnections(accountId), Times.Once);
            actual.Should().BeEquivalentTo(apiResponse.Select(model => new EmployerTransferConnection
            {
                FundingEmployerPublicHashedAccountId = model.FundingEmployerPublicHashedAccountId,
                FundingEmployerAccountName = model.FundingEmployerAccountName,
                FundingEmployerHashedAccountId = model.FundingEmployerHashedAccountId,
                FundingEmployerAccountId = model.FundingEmployerAccountId
            }));
        }
    }
}
