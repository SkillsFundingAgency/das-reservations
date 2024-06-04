using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Services
{
    public class WhenGettingTrustedEmployers
    {
        private const uint ExpectedUkPrn = 12345;

        private IReservationsOuterService _reservationsOuterService;
        private Mock<IReservationsOuterApiClient> _reservationsOuterApiClient;
        private List<Employer> _expectedEmployers;
        private IOptions<ReservationsOuterApiConfiguration> _options;

        [SetUp]
        public void Arrange()
        {
            _expectedEmployers = new List<Employer>
            {
                new Employer
                {
                    AccountId = 1,
                    AccountPublicHashedId = "ABC111",
                    AccountName = "account 1",
                    AccountLegalEntityId = 11,
                    AccountLegalEntityPublicHashedId = "DEF111",
                    AccountLegalEntityName = "entity 1"
                },
                new Employer
                {
                    AccountId = 2,
                    AccountPublicHashedId = "ABC222",
                    AccountName = "account 2",
                    AccountLegalEntityId = 22,
                    AccountLegalEntityPublicHashedId = "DEF222",
                    AccountLegalEntityName = "entity 2"
                }
            };

            _options = Mock.Of<IOptions<ReservationsOuterApiConfiguration>>(x => x.Value == new ReservationsOuterApiConfiguration());
            _reservationsOuterApiClient = new Mock<IReservationsOuterApiClient>();
            _reservationsOuterService = new ReservationsOuterService(_reservationsOuterApiClient.Object, _options);

            _reservationsOuterApiClient.Setup(c => c.Get<GetAccountProviderLegalEntitiesWithPermissionResponse>(
                It.Is<GetAccountProviderLegalEntitiesWithPermissionRequest>(r =>
                    r.Operations.Contains(Operation.CreateCohort) &&
                    r.Ukprn == ExpectedUkPrn))).ReturnsAsync(new GetAccountProviderLegalEntitiesWithPermissionResponse
                    {
                        AccountProviderLegalEntities = new[]
                {
                    new AccountProviderLegalEntity
                    {
                        AccountId = 1,
                        AccountPublicHashedId = "ABC111",
                        AccountLegalEntityId = 11,
                        AccountLegalEntityPublicHashedId = "DEF111",
                        AccountLegalEntityName = "entity 1",
                        AccountName = "account 1"
                    },
                    new AccountProviderLegalEntity
                    {
                        AccountId = 2,
                        AccountPublicHashedId = "ABC222",
                        AccountLegalEntityId = 22,
                        AccountLegalEntityPublicHashedId = "DEF222",
                        AccountLegalEntityName = "entity 2",
                        AccountName = "account 2"
                    }
                }
                    });
        }

        [Test]
        public async Task Then_Ukprn_Is_Required_To_Get_Provider_Permissions()
        {
            //Act
            try
            {
                await _reservationsOuterService.GetTrustedEmployers(default(uint));
            }
            catch (Exception)
            {
                //Swallow exception as we test for this in a different test
            };

            //Assert
            _reservationsOuterApiClient.Verify(c => c.Get<GetAccountProviderLegalEntitiesWithPermissionRequest>(
                It.IsAny<GetAccountProviderLegalEntitiesWithPermissionRequest>()), Times.Never);

            Assert.ThrowsAsync<ArgumentException>(() => _reservationsOuterService.GetTrustedEmployers(default(uint)));
        }

        [Test]
        public async Task Then_All_Trusted_Employers_Are_Returned()
        {
            //Act
            var result = await _reservationsOuterService.GetTrustedEmployers(ExpectedUkPrn);

            //Assert
            result.Should().BeEquivalentTo(_expectedEmployers);
        }
    }
}
