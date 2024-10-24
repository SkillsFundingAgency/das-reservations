using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.EmployerAccountService;

public class WhenCallingGetAccountUsers
{
    [Test, MoqAutoData]
    public async Task Then_Returns_All_Users_For_Account_From_Outer_Api(
        long accountId,
        GetAccountUsersApiResponse response,
        [Frozen] Mock<IReservationsOuterApiClient> mockApiClient,
        Infrastructure.Services.EmployerAccountService service)
    {
        mockApiClient
            .Setup(client => client.Get<GetAccountUsersApiResponse>(It.Is<GetAccountUsersRequest>(x=> x.GetUrl.EndsWith($"/accounts/{accountId}/users"))))
            .ReturnsAsync(response);

        var result = await service.GetAccountUsers(accountId);

        result.Should().BeEquivalentTo(response.AccountUsers.Select(model => (EmployerAccountUser)model));
    }
}