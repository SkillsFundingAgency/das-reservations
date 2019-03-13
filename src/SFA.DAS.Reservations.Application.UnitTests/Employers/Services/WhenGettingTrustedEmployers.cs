using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;
using SFA.DAS.ProviderRelationships.Types.Models;
using SFA.DAS.Reservations.Application.Employers.Services;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Services
{
    public class WhenGettingTrustedEmployers
    {
        private const uint ExpectedUkPrn = 12345;

        private ProviderPermissionsService _providerPermissionsService;
        private Mock<IProviderRelationshipsApiClient> _providerRelationsApiClient;
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

            _providerRelationsApiClient = new Mock<IProviderRelationshipsApiClient>();
            _providerPermissionsService = new ProviderPermissionsService(_providerRelationsApiClient.Object);

            _providerRelationsApiClient.Setup(c => c.GetAccountProviderLegalEntitiesWithPermission(
                It.Is<GetAccountProviderLegalEntitiesWithPermissionRequest>(r =>
                    r.Operation == Operation.CreateCohort &&
                    r.Ukprn == ExpectedUkPrn), It.IsAny<CancellationToken>())).ReturnsAsync(new GetAccountProviderLegalEntitiesWithPermissionResponse
            {
                AccountProviderLegalEntities = new[]
                {
                    new AccountProviderLegalEntityDto
                    {
                        AccountId = 1,
                        AccountPublicHashedId = "ABC111",
                        AccountLegalEntityId = 11,
                        AccountLegalEntityPublicHashedId = "DEF111",
                        AccountLegalEntityName = "entity 1",
                        AccountName = "account 1"
                    },
                    new AccountProviderLegalEntityDto
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
        public async Task Then_Will_Not_Call_Api_If_Ukprn_Is_Not_Set()
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
            _providerRelationsApiClient.Verify(c => c.GetAccountProviderLegalEntitiesWithPermission(
                It.IsAny<GetAccountProviderLegalEntitiesWithPermissionRequest>(), 
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void Then_Will_Throw_Exception_If_Ukprn_Is_Not_Set()
        {
            //Act & Assert
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
