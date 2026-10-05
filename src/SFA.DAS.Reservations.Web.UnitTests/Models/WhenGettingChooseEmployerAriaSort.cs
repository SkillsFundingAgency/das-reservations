using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenGettingChooseEmployerAriaSort
    {
        [Test]
        public void And_Sorted_Ascending_Then_Aria_Sort_Is_Ascending()
        {
            var viewModel = new ChooseEmployerViewModel
            {
                SortModel = new SortModel
                {
                    ReverseSort = false
                }
            };

            viewModel.AriaSortValue.Should().Be("ascending");
            viewModel.AriaSortHelp.Should().Be("A to Z");
        }

        [Test]
        public void And_Sorted_Descending_Then_Aria_Sort_Is_Descending()
        {
            var viewModel = new ChooseEmployerViewModel
            {
                SortModel = new SortModel
                {
                    ReverseSort = true
                }
            };

            viewModel.AriaSortValue.Should().Be("descending");
            viewModel.AriaSortHelp.Should().Be("Z to A");
        }
    }
}
