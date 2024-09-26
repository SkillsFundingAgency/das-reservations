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
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.EmployerAccountService;

public class WhenGettingAccountClaim
{
    [Test, MoqAutoData]
    public async Task Then_If_Gov_SignIn_Gets_Claim_Values_From_Outer_Api_And_Not_Suspended_Is_Marked_Correctly(
        string userId,
        string claimType,
        string email,
        GetUserAccountsResponse getUserAccountsResponse,
        [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
        [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
        [Frozen] Mock<IOptions<ReservationsOuterApiConfiguration>> outerApiConfiguration,
        Infrastructure.Services.EmployerAccountService employerAccountService)
    {
        getUserAccountsResponse.IsSuspended = false;
        outerApiConfiguration.Object.Value.ApiBaseUrl = "https://tempuri.org";
        var expectedRequest =
            new GetUserAccountsRequest(outerApiConfiguration.Object.Value.ApiBaseUrl, userId, email);
        reservationsOuterApiClient.Setup(x => x.Get<GetUserAccountsResponse>(
                It.Is<GetUserAccountsRequest>(c => c.GetUrl.Equals(expectedRequest.GetUrl))))
            .ReturnsAsync(getUserAccountsResponse);

        var actual = await employerAccountService.GetClaim(userId, claimType, email);

        var actualValue = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(actual.FirstOrDefault(c => c.Type.Equals(claimType)).Value);
        foreach (var employerIdentifier in actualValue)
        {
            employerIdentifier.Value.Should().BeEquivalentTo(getUserAccountsResponse.UserAccounts.SingleOrDefault(c => c.AccountId.Equals(employerIdentifier.Key)));
        }

        actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value.Should().BeNullOrEmpty();
        actual.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))?.Value.Should().Be(getUserAccountsResponse.UserId);
    }

    [Test, MoqAutoData]
    public async Task Then_If_Gov_SignIn_Gets_Claim_Values_From_Outer_Api_And_Suspended_Is_Marked_Correctly(
        string userId,
        string claimType,
        string email,
        GetUserAccountsResponse getUserAccountsResponse,
        [Frozen] Mock<IReservationsOuterApiClient> reservationsOuterApiClient,
        [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration,
        [Frozen] Mock<IOptions<ReservationsOuterApiConfiguration>> outerApiConfiguration,
        Infrastructure.Services.EmployerAccountService employerAccountService)
    {
        getUserAccountsResponse.IsSuspended = true;
        outerApiConfiguration.Object.Value.ApiBaseUrl = "https://tempuri.org";
        var expectedRequest =
            new GetUserAccountsRequest(outerApiConfiguration.Object.Value.ApiBaseUrl, userId, email);
        reservationsOuterApiClient.Setup(x => x.Get<GetUserAccountsResponse>(
                It.Is<GetUserAccountsRequest>(c => c.GetUrl.Equals(expectedRequest.GetUrl))))
            .ReturnsAsync(getUserAccountsResponse);

        var actual = await employerAccountService.GetClaim(userId, claimType, email);

        var actualValue = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(actual.FirstOrDefault(c => c.Type.Equals(claimType)).Value);
        foreach (var employerIdentifier in actualValue)
        {
            employerIdentifier.Value.Should().BeEquivalentTo(getUserAccountsResponse.UserAccounts.SingleOrDefault(c => c.AccountId.Equals(employerIdentifier.Key)));
        }

        actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value.Should().Be("Suspended");
        actual.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))?.Value.Should().Be(getUserAccountsResponse.UserId);
    }
}