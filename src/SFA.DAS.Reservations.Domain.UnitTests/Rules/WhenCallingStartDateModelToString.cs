using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Domain.UnitTests.Rules
{
    [TestFixture]
    public class WhenCallingStartDateModelToString
    {
        [Test, AutoData]
        public void Then_Dates_Are_Formatted_Correctly(
            StartDateModel model)
        {
            var expectedString = $"{model.StartDate:MMM yyyy} to {model.EndDate:MMM yyyy}";

            model.ToString().Should().Be(expectedString);
        }
    }
}