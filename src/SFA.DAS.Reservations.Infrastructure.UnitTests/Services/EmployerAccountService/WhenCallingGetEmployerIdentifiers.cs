using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.EmployerAccountService
{
    public class WhenCallingGetEmployerIdentifiers
    {
        private IEnumerable<AccountDetailViewModel> _accountDetailViewModels;

        [SetUp]
        public void SetUp()
        {
            /*
             setting up customisation of AccountDetailViewModel to avoid assignment of autoproperties
             that override List<T>, which causes intermittent failure (i.e. only when exceeding capacity)
             with the following error:
             System.ArgumentOutOfRangeException: capacity was less than the current size. 
            */
            var fixture = new Fixture();
            fixture.Customize<AccountDetailViewModel>(composer => composer
                .Without(model => model.LegalEntities)
                .Without(model => model.PayeSchemes));

            _accountDetailViewModels = fixture.CreateMany<AccountDetailViewModel>();
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Accounts_From_AccountApi(
            string userId,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            Infrastructure.Services.EmployerAccountService employerAccountService)
        {
            mockAccountApiClient
                .Setup(client => client.GetUserAccounts(It.IsAny<string>()))
                .ReturnsAsync(_accountDetailViewModels.ToList);

            await employerAccountService.GetEmployerIdentifiersAsync(userId);

            mockAccountApiClient.Verify(client => client.GetUserAccounts(userId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Maps_Accounts_To_EmployerIdentifiers(
            string userId,
            [Frozen] Mock<IAccountApiClient> mockAccountApiClient,
            Infrastructure.Services.EmployerAccountService employerAccountService)
        {
            mockAccountApiClient
                .Setup(client => client.GetUserAccounts(It.IsAny<string>()))
                .ReturnsAsync(_accountDetailViewModels.ToList);

            var result = await employerAccountService.GetEmployerIdentifiersAsync(userId);

            result.Should().BeEquivalentTo(_accountDetailViewModels.Select(model => new EmployerIdentifier
            {
                AccountId = model.HashedAccountId,
                EmployerName = model.DasAccountName
            }));
        }
    }
}