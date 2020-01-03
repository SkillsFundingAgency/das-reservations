using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Employers.Queries.GetLegalEntity;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Employers.Queries.GetLegalEntity
{
    [TestFixture]
    public class WhenGettingALegalEntity
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Legal_Entity(
            GetLegalEntityQuery query,
            AccountLegalEntity legalEntity,
            [Frozen] Mock<IApiClient> mockApiClient,
            GetLegalEntityQueryHandler handler)
        {
            
            mockApiClient
                .Setup(client => client.Get<AccountLegalEntity>(It.IsAny<GetAccountLegalEntityRequest>()))
                .ReturnsAsync(legalEntity);

            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.Get<AccountLegalEntity>(It.Is<GetAccountLegalEntityRequest>(r => r.Id.Equals(query.Id))), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Uses_correct_Api_Base_Url(
            GetLegalEntityQuery query,
            AccountLegalEntity legalEntity,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] Mock<IEncodingService> encodingService,
            [Frozen] ReservationsApiConfiguration configuration,
            GetLegalEntityQueryHandler handler)
        {
            mockOptions.Setup(c => c.Value).Returns(configuration);

            mockApiClient
                .Setup(client => client.Get<AccountLegalEntity>(It.IsAny<GetAccountLegalEntityRequest>()))
                .ReturnsAsync(legalEntity);
            
            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.Get<AccountLegalEntity>(It.Is<GetAccountLegalEntityRequest>(r => r.BaseUrl.Equals(configuration.Url))), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Hashes_The_Id(
            GetLegalEntityQuery query,
            AccountLegalEntity legalEntity,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<IEncodingService> encodingService,
            GetLegalEntityQueryHandler handler)
        {

            mockApiClient
                .Setup(client => client.Get<AccountLegalEntity>(It.IsAny<GetAccountLegalEntityRequest>()))
                .ReturnsAsync(legalEntity);
            
            await handler.Handle(query, CancellationToken.None);

            encodingService.Verify(service => service.Encode(legalEntity.AccountLegalEntityId,EncodingType.PublicAccountLegalEntityId), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_Legal_Entity(
            GetLegalEntityQuery query,
            AccountLegalEntity legalEntity,
            [Frozen] Mock<IApiClient> mockApiClient,
            GetLegalEntityQueryHandler handler)
        {
            
            mockApiClient
                .Setup(client => client.Get<AccountLegalEntity>(It.IsAny<GetAccountLegalEntityRequest>()))
                .ReturnsAsync(legalEntity);
            
            var result = await handler.Handle(query, CancellationToken.None);

            result.AccountLegalEntity.Should().Be(legalEntity);
        }
    }
}