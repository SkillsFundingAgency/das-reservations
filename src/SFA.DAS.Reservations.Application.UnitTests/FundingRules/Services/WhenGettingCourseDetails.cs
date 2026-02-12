using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Application.UnitTests.FundingRules.Services;

public class WhenGettingCourseDetails
{
    private IReservationsOuterService _service;
    private Mock<IReservationsOuterApiClient> _apiClient;
    private Mock<IOptions<ReservationsOuterApiConfiguration>> _options;
    private const string ExpectedBaseUrl = "https://test.local/";
    private GetCourseApiResponse _expectedCourse;
    private string _courseId;

    [SetUp]
    public void Arrange()
    {
        var fixture = new Fixture();

        _courseId = fixture.Create<string>();
        _expectedCourse = fixture.Create<GetCourseApiResponse>();

        _apiClient = new Mock<IReservationsOuterApiClient>();
        _apiClient.Setup(x =>
                x.Get<GetCourseApiResponse>(
                    It.Is<GetCourseApiRequest>(c =>
                        c.GetUrl.Equals(
                            $"{ExpectedBaseUrl}api/courses/{_courseId}"))))
            .ReturnsAsync(_expectedCourse);

        var config = new ReservationsOuterApiConfiguration
        {
            ApiBaseUrl = ExpectedBaseUrl
        };

        _options = new Mock<IOptions<ReservationsOuterApiConfiguration>>();
        _options.Setup(opt => opt.Value).Returns(config);

        _service = new ReservationsOuterService(_apiClient.Object, _options.Object);
    }

    [Test]
    public async Task Then_The_Course_Details_Are_Returned()
    {
        //Act
        var result = await _service.GetCourseDetails(_courseId);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_expectedCourse);
    }

    [Test]
    public async Task And_No_Course_Found_Then_Returns_Null()
    {
        //Arrange
        var exception = new WebException();
        _apiClient.Setup(x =>
                x.Get<GetCourseApiResponse>(
                    It.Is<GetCourseApiRequest>(c =>
                        c.GetUrl.Equals(
                            $"{ExpectedBaseUrl}api/courses/{_courseId}"))))
            .ReturnsAsync((GetCourseApiResponse)null);

        //Act
        var result = await _service.GetCourseDetails(_courseId);

        //Assert
        result.Should().BeNull();
    }
}
