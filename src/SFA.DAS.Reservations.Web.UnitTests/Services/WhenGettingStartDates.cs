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
                date.Year == now.Year);
        }

        [Test, MoqAutoData, Ignore("todo")]
        public async Task Then_Returns_The_Next_Five_Months_After_This_Month(
            [Frozen] Mock<ICurrentDateTime> mockCurrentDateTime, 
            StartDateService startDateService)
        {
            var now = mockCurrentDateTime.Object.Now;
            var dates = await startDateService.GetStartDates();

            // todo: check other months in loop
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
            await Task.CompletedTask;
            var datesToReturn = new List<DateTime>();
            datesToReturn.Add(_currentDateTime.Now);
            return datesToReturn;
        }
    }
}