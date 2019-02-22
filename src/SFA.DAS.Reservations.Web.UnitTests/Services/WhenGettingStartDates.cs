using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.UnitTests.Services
{
    [TestFixture]
    public class WhenGettingStartDates
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_A_Date_Based_On_The_Current_Month(
            [Frozen] Mock<ICurrentDateTime> mockCurrentDateTime, 
            StartDateService startDateService)
        {
            var now = mockCurrentDateTime.Object.Now;
            var expectedDate = now.AddDays(1 - now.Day).Date;

            var dates = await startDateService.GetStartDates();

            dates.ToList().Should().Contain(expectedDate);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_The_Next_Five_Months_After_This_Month(
            [Frozen] Mock<ICurrentDateTime> mockCurrentDateTime, 
            StartDateService startDateService)
        {
            var now = mockCurrentDateTime.Object.Now;
            var expectedDates = new List<DateTime>();
            for (var i = 0; i < 6; i++)
            {
                expectedDates.Add(now.AddMonths(i).AddDays(1-now.Day).Date);
            }

            var dates = await startDateService.GetStartDates();

            dates.Count().Should().Be(6);
            dates.Should().BeEquivalentTo(expectedDates);
        }
    }
}