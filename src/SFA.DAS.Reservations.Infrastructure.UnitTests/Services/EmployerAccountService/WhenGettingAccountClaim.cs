using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Testing.AutoFixture;
using EmployerIdentifier = SFA.DAS.Reservations.Domain.Authentication.EmployerIdentifier;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.EmployerAccountService
{
    public class WhenGettingAccountClaim
    {
        [Test, MoqAutoData]
        public async Task Then_If_Gov_SignIn_Gets_Claim_Values_From_Outer_Api_And_Not_Suspended_Is_Marked_Correctly(
            string userId,
            string claimType,
            string email,
            GetUserAccountsResponse getUserAccountsResponse,
            [Frozen] Mock<IAccountApiClient> accountsApi,
            [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
            [Frozen] Mock<IOptions<ReservationsOuterApiConfiguration>> outerApiConfiguration,
            Infrastructure.Services.EmployerAccountService employerAccountService)
        {
            getUserAccountsResponse.IsSuspended = false;
            outerApiConfiguration.Object.Value.ApiBaseUrl = "https://tempuri.org";
            var expectedRequest =
                new GetUserAccountsRequest(outerApiConfiguration.Object.Value.ApiBaseUrl, userId, email);
            configuration.Object.Value.UseGovSignIn = true;
            reservationsOuterApiClient.Setup(x => x.Get<GetUserAccountsResponse>(
                    It.Is<GetUserAccountsRequest>(c=>c.GetUrl.Equals(expectedRequest.GetUrl))))
                .ReturnsAsync(getUserAccountsResponse);

            var actual = await employerAccountService.GetClaim(userId, claimType, email);

            var actualValue = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(actual.FirstOrDefault(c=>c.Type.Equals(claimType)).Value);
            foreach (var employerIdentifier in actualValue)
            {
                employerIdentifier.Value.Should().BeEquivalentTo(getUserAccountsResponse.UserAccounts.SingleOrDefault(c => c.AccountId.Equals(employerIdentifier.Key)));
            }
            accountsApi.Verify(x=>x.GetUserAccounts(It.IsAny<string>()), Times.Never);
            accountsApi.Verify(x=>x.GetAccountUsers(It.IsAny<string>()), Times.Never);
            actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value.Should().BeNullOrEmpty();
            actual.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))?.Value.Should().Be(getUserAccountsResponse.UserId);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Gov_SignIn_Gets_Claim_Values_From_Outer_Api_And_Suspended_Is_Marked_Correctly(
            string userId,
            string claimType,
            string email,
            GetUserAccountsResponse getUserAccountsResponse,
            [Frozen] Mock<IAccountApiClient> accountsApi,
            [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
            [Frozen] Mock<IOptions<ReservationsOuterApiConfiguration>> outerApiConfiguration,
            Infrastructure.Services.EmployerAccountService employerAccountService)
        {
            getUserAccountsResponse.IsSuspended = true;
            outerApiConfiguration.Object.Value.ApiBaseUrl = "https://tempuri.org";
            var expectedRequest =
                new GetUserAccountsRequest(outerApiConfiguration.Object.Value.ApiBaseUrl, userId, email);
            configuration.Object.Value.UseGovSignIn = true;
            reservationsOuterApiClient.Setup(x => x.Get<GetUserAccountsResponse>(
                    It.Is<GetUserAccountsRequest>(c=>c.GetUrl.Equals(expectedRequest.GetUrl))))
                .ReturnsAsync(getUserAccountsResponse);

            var actual = await employerAccountService.GetClaim(userId, claimType, email);

            var actualValue = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(actual.FirstOrDefault(c=>c.Type.Equals(claimType)).Value);
            foreach (var employerIdentifier in actualValue)
            {
                employerIdentifier.Value.Should().BeEquivalentTo(getUserAccountsResponse.UserAccounts.SingleOrDefault(c => c.AccountId.Equals(employerIdentifier.Key)));
            }
            accountsApi.Verify(x=>x.GetUserAccounts(It.IsAny<string>()), Times.Never);
            accountsApi.Verify(x=>x.GetAccountUsers(It.IsAny<string>()), Times.Never);
            actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value.Should().Be("Suspended");
            actual.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))?.Value.Should().Be(getUserAccountsResponse.UserId);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_If_Not_Gov_SignIn_Gets_Claims_From_Accounts_Api(
            string userId,
            string claimType,
            string email,
            string hashedId,
            string accountName,
            List<TeamMemberViewModel> teamMemberViewModels,
            [Frozen] Mock<IAccountApiClient> accountsApi,
            [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
            Infrastructure.Services.EmployerAccountService employerAccountService)
        {
            var accountDetailViewModel = new List<AccountDetailViewModel>();
            accountDetailViewModel.Add(new AccountDetailViewModel
            {
                HashedAccountId = hashedId,
                DasAccountName = accountName,
            });
            configuration.Object.Value.UseGovSignIn = false;
            teamMemberViewModels.First().UserRef = userId;
            accountsApi.Setup(x => x.GetUserAccounts(userId)).ReturnsAsync(accountDetailViewModel);
            accountsApi.Setup(x => x.GetAccountUsers(accountDetailViewModel.First().HashedAccountId)).ReturnsAsync(teamMemberViewModels);
            
            var actual = await employerAccountService.GetClaim(userId, claimType, email);
            
            var actualValue = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(actual.FirstOrDefault(c=>c.Type.Equals(claimType)).Value);
            actualValue.First().Value.AccountId.Should().Be(accountDetailViewModel.First().HashedAccountId);
            actualValue.First().Value.Role.Should().Be(teamMemberViewModels.First().Role);
            actualValue.First().Value.EmployerName.Should().Be(accountDetailViewModel.First().DasAccountName);
            reservationsOuterApiClient.Verify(x=>x.Get<GetUserAccountsResponse>(It.IsAny<GetUserAccountsRequest>()), Times.Never);
            actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value.Should().BeNullOrEmpty();
        }
    }
}