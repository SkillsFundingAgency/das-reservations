using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    [TestFixture]
    public class WhenConstructingACourseViewModel
    {
        [Test, AutoData]
        public void Then_Sets_Id(
            Course course)
        {
            var viewModel = new CourseViewModel(course);
            viewModel.Id.Should().Be(course.Id);
        }

        [Test, AutoData]
        public void Then_Sets_Title(
            Course course)
        {
            var viewModel = new CourseViewModel(course);
            viewModel.Title.Should().Be(course.Title);
        }

        [Test, AutoData]
        public void Then_Sets_Level(
            Course course)
        {
            var viewModel = new CourseViewModel(course);
            viewModel.Level.Should().Be(course.Level);
        }

        [Test, AutoData]
        public void Then_Sets_Selected(
            Course course)
        {
            var courseId = course.Id;
            var viewModel = new CourseViewModel(course, courseId);
            viewModel.Selected.Should().Be("selected");
        }

        [Test, AutoData]
        public void And_Not_Match_Course_Then_Selected_Is_Null(
            Course course)
        {
            var courseId = $"not-{course.Id}";
            var viewModel = new CourseViewModel(course, courseId);
            viewModel.Selected.Should().BeNull();
        }

        [Test, AutoData]
        public void And_Null_CourseId_Then_Selected_Is_Null(
            Course course)
        {
            var viewModel = new CourseViewModel(course);
            viewModel.Selected.Should().BeNull();
        }
    }
}