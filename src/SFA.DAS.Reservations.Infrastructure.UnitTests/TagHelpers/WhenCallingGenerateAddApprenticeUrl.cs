using AutoFixture.NUnit3;
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
        public void Then_Uses_ApprenticeUrl_And_Params_To_Build_Url(
            UrlParameters urlParameters,
            [Frozen] ReservationsWebConfiguration config,
            ProviderExternalUrlHelper urlHelper)
        {
            var originalConfigUrl = config.ApprenticeUrl;
            config.ApprenticeUrl = $"https://{config.ApprenticeUrl}";
            
            var actualUrl = urlHelper.GenerateAddApprenticeUrl(urlParameters);
            
            Assert.AreEqual(
                $"https://{urlParameters.SubDomain}.{originalConfigUrl}/{urlParameters.Folder}/{urlParameters.Id}/{urlParameters.Controller}/{urlParameters.Action}{urlParameters.QueryString}", 
                actualUrl);
        }
    }
}