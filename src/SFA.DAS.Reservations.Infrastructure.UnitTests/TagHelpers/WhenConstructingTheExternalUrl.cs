using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.TagHelpers
{
    public class WhenConstructingTheExternalUrl
    {
        private Mock<IOptions<ReservationsWebConfiguration>> _reservationsConfig;
        private ProviderExternalUrlHelper _helper;
        private Mock<IConfiguration> _config;

        [SetUp]
        public void Arrange()
        {
            _reservationsConfig = new Mock<IOptions<ReservationsWebConfiguration>>();
            _reservationsConfig.Setup(x => x.Value.DashboardUrl).Returns("https://test.local");
            _config = new Mock<IConfiguration>();
            _config.Setup(x => x["AuthType"]).Returns("provider");
            _helper = new ProviderExternalUrlHelper(_reservationsConfig.Object, _config.Object);
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
        public void Then_The_Folder_Is_Included_In_The_Url_And_Correct_Base_Url_Based_On_Auth_Type()
        {
            //Arrange
            var controller = "test-controller";
            var subDomain = "testDomain";
            var folder = "test-folder";
            _reservationsConfig = new Mock<IOptions<ReservationsWebConfiguration>>();
            _reservationsConfig.Setup(x => x.Value.DashboardUrl).Returns("https://test.local/account");
            _reservationsConfig.Setup(x => x.Value.EmployerDashboardUrl).Returns("https://test.local.dashboard/");
            _config = new Mock<IConfiguration>();
            _config.Setup(x => x["AuthType"]).Returns("employer");
            _helper = new ProviderExternalUrlHelper(_reservationsConfig.Object, _config.Object);

            //Act
            var actual = _helper.GenerateUrl(controller: controller, subDomain: subDomain, folder:folder);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual($"https://{subDomain}.test.local.dashboard/{folder}/{controller}", actual);
        }
    }
}