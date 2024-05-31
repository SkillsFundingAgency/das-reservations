using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.ProviderRelationships;
using SFA.DAS.Reservations.Domain.ProviderRelationships.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Services
{
    public class WhenGettingTrustedEmployers
    {
        private const uint ExpectedUkPrn = 12345;

        private ProviderPermissionsService _providerPermissionsService;
        private Mock<IProviderRelationshipsOuterApiClient> _providerRelationsApiClient;
        private List<Employer> _expectedEmployers;
        private IOptions<ProviderRelationshipsOuterApiConfiguration> _options;

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

            _options = Mock.Of<IOptions<ProviderRelationshipsOuterApiConfiguration>>(x => x.Value == new ProviderRelationshipsOuterApiConfiguration());
            _providerRelationsApiClient = new Mock<IProviderRelationshipsOuterApiClient>();
            _providerPermissionsService = new ProviderPermissionsService(_providerRelationsApiClient.Object, _options);

            _providerRelationsApiClient.Setup(c => c.Get<GetAccountProviderLegalEntitiesWithPermissionResponse>(
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
                await _providerPermissionsService.GetTrustedEmployers(default(uint));
            }
            catch (Exception)
            {
                //Swallow exception as we test for this in a different test
            };

            //Assert
            _providerRelationsApiClient.Verify(c => c.Get<GetAccountProviderLegalEntitiesWithPermissionRequest>(
                It.IsAny<GetAccountProviderLegalEntitiesWithPermissionRequest>()), Times.Never);

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
