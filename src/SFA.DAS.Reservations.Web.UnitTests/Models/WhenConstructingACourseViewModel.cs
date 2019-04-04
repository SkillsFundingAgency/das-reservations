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
        public void Then_Sets_Description(
            Course course)
        {
            var viewModel = new CourseViewModel(course);
            viewModel.Description.Should().Be(course.CourseDescription);
        }

        [Test]
        public void Then_Sets_Selected()
        {
            var course = new Course("12","Test",1);
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

        [Test]
        public void Then_If_The_Course_Is_Null_A_New_ViewModel_Is_Returned_With_Description_Set_To_Unknown()
        {
            var viewModel = new CourseViewModel(null);
            viewModel.Should().NotBeNull();
            viewModel.Description.Should().Be("Unknown");
        }

        [Test]
        public void Then_If_The_Course_Is_An_Empty_Object_The_Select_Is_Null()
        {
            var viewModel = new CourseViewModel(new Course(null,null,0));
            viewModel.Should().NotBeNull();
            viewModel.Description.Should().Be("Unknown");
            viewModel.Selected.Should().BeNull();
        }
    }
}