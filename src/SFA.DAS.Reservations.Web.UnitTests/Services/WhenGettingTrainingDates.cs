using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAvailableDates;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Services
{
    [TestFixture]
    public class WhenGettingTrainingDates
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_The_Available_Dates_From_The_Query(
            long accountLegalEntityId,
            [Frozen] Mock<IMediator> mockMediator,
            TrainingDateService trainingDateService)
        {
            await trainingDateService.GetTrainingDates(accountLegalEntityId);

            mockMediator.Verify(x => x.Send(
                It.Is<GetAvailableDatesQuery>(query => query.AccountLegalEntityId == accountLegalEntityId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_The_Returned_Dates_Are_Mapped_To_The_Model(
            long accountLegalEntityId,
            [Frozen] Mock<IMediator> mockMediator,
            IList<TrainingDateModel> expectedAvailableDates,
            TrainingDateModel previousMonth,
            TrainingDateService trainingDateService)
        {
            mockMediator.Setup(x => x.Send(It.IsAny<GetAvailableDatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAvailableDatesResult
                {
                    PreviousMonth = previousMonth,
                    AvailableDates = expectedAvailableDates
                });

            var dates = await trainingDateService.GetTrainingDates(accountLegalEntityId);

            Assert.IsNotNull(dates);
            Assert.AreEqual(expectedAvailableDates.Count, dates.AvailableDates.Count);
            Assert.AreEqual(previousMonth, dates.PreviousMonth);
        }
    }
}