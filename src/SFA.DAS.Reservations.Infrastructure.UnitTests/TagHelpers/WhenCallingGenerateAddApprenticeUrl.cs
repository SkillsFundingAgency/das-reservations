using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.TagHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.TagHelpers
{
    [TestFixture]
    public class WhenCallingGenerateAddApprenticeUrl
    {
        [Test, MoqAutoData]
        public void Then_Uses_ApprenticeUrl_And_Params_To_Build_Provider_Url(
            UrlParameters urlParameters,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("provider");

            var originalConfigUrl = webConfig.ApprenticeUrl;
            webConfig.ApprenticeUrl = $"https://{webConfig.ApprenticeUrl}";
            
            var actualUrl = urlHelper.GenerateAddApprenticeUrl(urlParameters);
            
            Assert.AreEqual(
                $"https://{urlParameters.SubDomain}.{originalConfigUrl}/{urlParameters.Folder}/{urlParameters.Id}/{urlParameters.Controller}/{urlParameters.Action}{urlParameters.QueryString}", 
                actualUrl);
        }

        [Test, MoqAutoData]
        public void Then_Uses_EmployerApprenticeUrl_And_Params_To_Build_Employer_Url(
            UrlParameters urlParameters,
            [Frozen] ReservationsWebConfiguration webConfig,
            [Frozen] Mock<IConfiguration> config,
            ExternalUrlHelper urlHelper)
        {
            config.Setup(x => x["AuthType"]).Returns("employer");

            var originalConfigUrl = webConfig.EmployerApprenticeUrl;
            webConfig.EmployerApprenticeUrl = $"https://{webConfig.EmployerApprenticeUrl}";

            var actualUrl = urlHelper.GenerateAddApprenticeUrl(urlParameters);

            Assert.AreEqual(
                $"https://{urlParameters.SubDomain}.{originalConfigUrl}/{urlParameters.Folder}/{urlParameters.Id}/{urlParameters.Controller}/{urlParameters.Action}{urlParameters.QueryString}",
                actualUrl);
        }
    }
}
