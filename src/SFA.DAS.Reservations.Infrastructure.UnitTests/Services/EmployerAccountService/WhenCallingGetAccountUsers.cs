using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Testing.AutoFixture;


namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services.EmployerAccountService
{
    public class WhenCallingGetAccountUsers
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_All_Users_For_Account_From_Account_Api(
            long accountId,
            List<TeamMemberViewModel> teamMembers,
            [Frozen] Mock<IAccountApiClient> mockApiClient,
            Infrastructure.Services.EmployerAccountService service)
        {
            mockApiClient
                .Setup(client => client.GetAccountUsers(accountId))
                .ReturnsAsync(teamMembers);

            var result = await service.GetAccountUsers(accountId);

            result.Should().BeEquivalentTo(teamMembers);
        }
    }
}