using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Providers.Api;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Services
{
    public class WhenGettingTrustedEmployers
    {
        private const uint ExpectedUkPrn = 12345;

        private ProviderPermissionsService _providerPermissionsService;
        private Mock<IReservationsOuterService> _reservationsOuterService;
        private List<Employer> _expectedEmployers;

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

            _reservationsOuterService = new Mock<IReservationsOuterService>();
            _providerPermissionsService = new ProviderPermissionsService(_reservationsOuterService.Object);

            _reservationsOuterService.Setup(c => c.GetAccountProviderLegalEntitiesWithCreateCohort(
                ExpectedUkPrn)).ReturnsAsync(new GetAccountLegalEntitiesForProviderResponse
                {
                    AccountProviderLegalEntities = new List<GetAccountLegalEntitiesForProviderItem>
                {
                    new GetAccountLegalEntitiesForProviderItem
                    {
                        AccountId = 1,
                        AccountPublicHashedId = "ABC111",
                        AccountLegalEntityId = 11,
                        AccountLegalEntityPublicHashedId = "DEF111",
                        AccountLegalEntityName = "entity 1",
                        AccountName = "account 1"
                    },
                    new GetAccountLegalEntitiesForProviderItem
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
                await _providerPermissionsService.GetTrustedEmployers(default(uint));
            }
            catch (Exception)
            {
                //Swallow exception as we test for this in a different test
            };

            //Assert
            _reservationsOuterService.Verify(c => c.GetAccountProviderLegalEntitiesWithCreateCohort(
                ExpectedUkPrn), Times.Never);

            Assert.ThrowsAsync<ArgumentException>(() => _providerPermissionsService.GetTrustedEmployers(default(uint)));
        }

        [Test]
        public async Task Then_All_Trusted_Employers_Are_Returned()
        {
            //Act
            var result = await _providerPermissionsService.GetTrustedEmployers(ExpectedUkPrn);

            //Assert
            result.Should().BeEquivalentTo(_expectedEmployers);
        }
    }
}
