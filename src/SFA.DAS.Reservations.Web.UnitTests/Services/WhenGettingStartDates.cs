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
            var dates = await startDateService.GetStartDates();

            dates.ToList().Should().Contain(date => 
                date.Month == now.Month && 
                date.Year == now.Year &&
                date.Day == 1 &&
                date.Hour == 0);
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

    public class StartDateService : IStartDateService
    {
        private readonly ICurrentDateTime _currentDateTime;

        public StartDateService(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

        public async Task<IEnumerable<DateTime>> GetStartDates()
        {
            await Task.CompletedTask; // this service will need to read rules at some point in the future.

            var now = _currentDateTime.Now;
            var datesToReturn = new List<DateTime>();
            for (var i = 0; i < 6; i++)
            {
                var dateToAdd = now.AddMonths(i).AddDays(1-now.Day).Date;
                datesToReturn.Add(dateToAdd);
            }
            return datesToReturn;
        }
    }
}