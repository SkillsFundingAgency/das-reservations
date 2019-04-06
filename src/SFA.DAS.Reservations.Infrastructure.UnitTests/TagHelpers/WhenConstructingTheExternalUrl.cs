using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.Configuration.Configuration;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.TagHelpers
{
    public class WhenConstructingTheExternalUrl
    {
        private Mock<IOptions<ReservationsWebConfiguration>> _config;
        private ProviderExternalUrlHelper _helper;

        [SetUp]
        public void Arrange()
        {
            _config = new Mock<IOptions<ReservationsWebConfiguration>>();
            _config.Setup(x => x.Value.DashboardUrl).Returns("https://test.local");
            _helper = new ProviderExternalUrlHelper(_config.Object);
        }

        [TestCase("https://test.local")]
        [TestCase("https://test.local/")]
        public void Then_The_Url_Is_Built_From_Action_Controller_And_External_Url(string expectedBaseUrl)
        {
            //Arrange
            var action = "test-action";
            var controller = "test-controller";

            //Act
            var actual = _helper.GenerateUrl("123", controller, action);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual($"https://test.local/123/{controller}/{action}", actual);
        }

        [Test]
        public void Then_The_Url_Is_Built_From_Action_Controller_And_External_Url_And_IncludesId_If_Supplied()
        {
            //Arrange
            var action = "test-action";
            var controller = "test-controller";
            var id = "ABC123";

            //Act
            var actual = _helper.GenerateUrl(id, controller, action);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual($"https://test.local/{id}/{controller}/{action}", actual);
        }

        [Test]
        public void Then_The_Url_Builds_From_Optional_Parameters()
        {
            //Arrange
            var controller = "test-controller";
            var id = "ABC123";

            //Act
            var actual = _helper.GenerateUrl(id, controller);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual($"https://test.local/{id}/{controller}", actual);
        }

        [Test]
        public void Then_The_Url_Builds_From_Controller()
        {
            //Arrange
            var controller = "test-controller";

            //Act
            var actual = _helper.GenerateUrl("", controller);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual($"https://test.local/{controller}", actual);
        }

        [Test]
        public void Then_The_SubDomain_Is_Set_If_Passed()
        {
            //Arrange
            var controller = "test-controller";
            var subDomain = "testDomain";

            //Act
            var actual = _helper.GenerateUrl(controller: controller, subDomain: subDomain);
            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual($"https://{subDomain}.test.local/{controller}", actual);
        }

        [Test]
        public void Then_The_Folder_Is_Included_In_The_Url()
        {
            //Arrange
            var controller = "test-controller";
            var subDomain = "testDomain";
            var folder = "test-folder";

            //Act
            var actual = _helper.GenerateUrl(controller: controller, subDomain: subDomain, folder:folder);
            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual($"https://{subDomain}.test.local/{folder}/{controller}", actual);
        }
    }
}