using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries.GetAccountUsers;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries.GetAccountUsers
{
    public class WhenGettingAccountUsers
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_Users_From_Service(
            GetAccountUsersQuery query,
            long accountId,
            List<EmployerAccountUser> users,
            [Frozen] Mock<IEmployerAccountService> mockEmployerAccountService,
            GetAccountUsersQueryHandler handler)
        {
            mockEmployerAccountService
                .Setup(service => service.GetAccountUsers(query.AccountId))
                .ReturnsAsync(users);

            var result = await handler.Handle(query, CancellationToken.None);

            result.AccountUsers.Should().BeEquivalentTo(users);
        }
    }
}