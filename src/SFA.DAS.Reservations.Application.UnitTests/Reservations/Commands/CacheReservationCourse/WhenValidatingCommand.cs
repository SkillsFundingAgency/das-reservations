﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Services;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CacheReservationCourse
{
    public class WhenValidatingCommand
    {
        private Mock<ICourseService> _courseService;
        private CacheReservationCourseCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _courseService = new Mock<ICourseService>();

            _courseService.Setup(s => s.CourseExists(It.IsAny<string>())).ReturnsAsync(true);

            _validator = new CacheReservationCourseCommandValidator(_courseService.Object);
        }

        [Test]
        public async Task Then_If_ReservationId_Is_Invalid_Then_Fail()
        {
            var command = new  CacheReservationCourseCommand
            {
                Id = Guid.Empty,
                SelectedCourseId = "1"
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof( CacheReservationCourseCommand.Id))
                .WhoseValue.Should().Be($"{nameof( CacheReservationCourseCommand.Id)} has not been supplied");
        }

        [Test]
        public async Task Then_If_CourseId_Not_Set_Then_Fail()
        {
            _courseService.Setup(s => s.CourseExists(It.IsAny<string>())).ReturnsAsync(false);

            var command = new  CacheReservationCourseCommand
            {
                Id = Guid.NewGuid(),
                SelectedCourseId = ""
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CacheReservationCourseCommand.SelectedCourseId))
                .WhoseValue.Should().Be("Select which apprenticeship training your apprentice will take");
        }

        
        [Test]
        public async Task Then_If_CourseId_Is_Invalid_Then_Fail()
        {
            _courseService.Setup(s => s.CourseExists(It.IsAny<string>())).ReturnsAsync(false);

            var command = new  CacheReservationCourseCommand
            {
                Id = Guid.NewGuid(),
                SelectedCourseId = "123"
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof( CacheReservationCourseCommand.SelectedCourseId))
                .WhoseValue.Should().Be("Selected course does not exist");
        }

        [Test]
        public async Task And_All_Fields_Invalid_Then_Returns_All_Errors()
        {
            _courseService.Setup(s => s.CourseExists(It.IsAny<string>())).ReturnsAsync(false);

            var command = new  CacheReservationCourseCommand{ SelectedCourseId = "INVALID" };

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(2);
            result.ValidationDictionary
                .Should().ContainKey(nameof( CacheReservationCourseCommand.Id))
                .And.ContainKey(nameof( CacheReservationCourseCommand.SelectedCourseId));
        }

        [Test]
        public async Task And_All_Fields_Valid_Then_Valid()
        {
            var command = new  CacheReservationCourseCommand
            {
                Id = Guid.NewGuid(),
                SelectedCourseId = "1"
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
