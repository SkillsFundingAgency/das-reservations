using System.Web;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers.Api;

namespace SFA.DAS.Reservations.Domain.UnitTests.Employers
{
    public class WhenBuildingGetUserAccountsRequest
    {
        [Test, AutoData]
        public void Then_The_Url_Is_Constructed_Correctly(string email, string userId, string baseUrl)
        {
            userId = userId + "$£%!: " + userId;
            email = email + "$£%!: " + email;
            
            var actual = new GetUserAccountsRequest(baseUrl, userId, email);

            actual.GetUrl.Should().Be($"{baseUrl}api/accountusers/{HttpUtility.UrlEncode(userId)}/accounts?email={HttpUtility.UrlEncode(email)}");
        }
    }
}