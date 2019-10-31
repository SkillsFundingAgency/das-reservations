using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Commitments.Services;
using SFA.DAS.Reservations.Domain.Commitments;
using SFA.DAS.Reservations.Domain.Commitments.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Commitments.Services
{
    public class WhenIGetCohort
    {
        [Test, MoqAutoData]
        public async Task ThenWillGetCohortFromCommitmentsApi(
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<IOptions<CommitmentsApiConfiguration>> mockOptions,
            CommitmentService service,
            long cohortId )
        {
            //Act
            await service.GetCohort(cohortId);

            //Assert
            mockApiClient.Verify(client => client.Get<Cohort>(
                    It.Is<IGetApiRequest>(request =>
                        request.GetUrl.StartsWith(mockOptions.Object.Value.ApiBaseUrl) &&
                        request.GetUrl.Contains(cohortId.ToString()))),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ThenWillReturnCohortFromCommitmentsApi(
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> mockOptions,
            CommitmentService service,
            Cohort cohort)
        {
            //Arrange
            mockApiClient.Setup(c => c.Get<Cohort>(It.IsAny<GetCohortRequest>()))
                .ReturnsAsync(cohort);

            //Act
            var result = await service.GetCohort(cohort.CohortId);

            //Assert
           Assert.AreEqual(cohort, result);
        }

        [Test, MoqAutoData]
        public void ThenWillThrowExceptionIFOneOccurs(
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> mockOptions,
            CommitmentService service,
            Cohort cohort,
            Exception exception)
        {
            //Arrange
            mockApiClient.Setup(c => c.Get<Cohort>(It.IsAny<GetCohortRequest>()))
                .Throws(exception);

            //Act
            var actualException = Assert.ThrowsAsync<Exception>(() => service.GetCohort(cohort.CohortId));

            //Assert
            Assert.AreEqual(exception, actualException);
        }
    }
}
