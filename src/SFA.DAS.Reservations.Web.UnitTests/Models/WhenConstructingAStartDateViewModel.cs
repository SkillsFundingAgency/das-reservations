using AutoFixture.NUnit3;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    [TestFixture]
    public class WhenConstructingAStartDateViewModel
    {
        [Test, AutoData]
        public void Then_Sets_Id(
            [Frozen] StartDateModel model,
            StartDateViewModel viewModel)
        {
            viewModel.Id.Should().Be($"{model.StartDate:yyyy-MM}");
        }

        [Test, AutoData]
        public void Then_Sets_Value(
            [Frozen] StartDateModel model,
            StartDateViewModel viewModel)
        {
            viewModel.Value.Should().Be(JsonConvert.SerializeObject(model));
        }

        [Test, AutoData]
        public void Then_Sets_Label(
            [Frozen] StartDateModel model,
            StartDateViewModel viewModel)
        {
            viewModel.Label.Should().Be($"{model.StartDate:MMMM yyyy}");
        }

        [Test, AutoData]
        public void Then_Sets_Checked(
            StartDateModel model)
        {
            var startDate = $"{model.StartDate:yyyy-MM}";
            var viewModel = new StartDateViewModel(model, startDate);
            viewModel.Checked.Should().Be("checked");
        }

        [Test, AutoData]
        public void And_Not_Match_StartDate_Then_Checked_Is_Null(
            StartDateModel model)
        {
            var startDate = $"{model.StartDate.AddMonths(1):yyyy-MM}";
            var viewModel = new StartDateViewModel(model, startDate);
            viewModel.Checked.Should().BeNull();
        }

        [Test, AutoData]
        public void And_Null_StartDate_Then_Checked_Is_Null(
            StartDateModel model)
        {
            var viewModel = new StartDateViewModel(model);
            viewModel.Checked.Should().BeNull();
        }
    }
}