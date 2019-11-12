using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenGettingPagedRecordsFrom
    {
        [Test, AutoData]
        public void And_PageNumber_1_Then_Should_Be_1(ManageReservationsFilterModel filterModel)
        {
            filterModel.PageNumber = 1;

            filterModel.PagedRecordsFrom.Should().Be(1);
        }

        [Test, AutoData]
        public void And_PageNumber_2_Then_Should_Be_PageSize_Plus_1(ManageReservationsFilterModel filterModel)
        {
            filterModel.PageNumber = 2;

            filterModel.PagedRecordsFrom.Should().Be(ManageReservationsFilterModel.PageSize+1);
        }

        [Test, AutoData]
        public void And_PageNumber_3_Then_Should_Be_Double_PageSize_Plus_1(ManageReservationsFilterModel filterModel)
        {
            filterModel.PageNumber = 3;

            filterModel.PagedRecordsFrom.Should().Be(2*ManageReservationsFilterModel.PageSize+1);
        }
    }
}