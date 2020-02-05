using NUnit.Framework;
using SFA.DAS.Reservations.Web.Extensions;

namespace SFA.DAS.Reservations.Web.UnitTests.Extensions.StringExtensionTests
{
    public class WhenConvertingToBoolean
    {
        [TestCase(null, null)]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("", null)]
        [TestCase("   ", null)]
        [TestCase("not a boolean", null)]
        public void ThenShouldReturnCorrectValue(string value, bool? expectedResult)
        {
            //Act
            var result = value.ToBoolean();

            //Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}
