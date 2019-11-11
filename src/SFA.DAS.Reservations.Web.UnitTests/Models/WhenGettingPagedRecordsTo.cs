using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenGettingPagedRecordsTo
    {
        [Test, AutoData]
        public void And_PageNumber_1_Then_Should_Be_PageSize_Plus_1(ManageReservationsFilterModel filterModel)
        {
            filterModel.PageNumber = 1;
            filterModel.TotalNumberOfRecords = 20 * filterModel.PageSize;

            filterModel.PagedRecordsTo.Should().Be(filterModel.PageSize + 1);
        }

        [Test, AutoData]
        public void And_PageNumber_2_Then_Should_Be_PageSize_Plus_1(ManageReservationsFilterModel filterModel)
        {
            filterModel.PageNumber = 2;
            filterModel.TotalNumberOfRecords = 20 * filterModel.PageSize;

            filterModel.PagedRecordsTo.Should().Be(2*filterModel.PageSize+1);
        }

        [Test, AutoData]
        public void And_PageNumber_3_Then_Should_Be_Double_PageSize_Plus_1(ManageReservationsFilterModel filterModel)
        {
            filterModel.PageNumber = 3;
            filterModel.TotalNumberOfRecords = 20 * filterModel.PageSize;

            filterModel.PagedRecordsTo.Should().Be(3*filterModel.PageSize+1);
        }

        [Test, AutoData]
        public void And_TotalRecords_Less_Than_Calculated_PagedRecordsTo_Then_Is_TotalRecords(ManageReservationsFilterModel filterModel)
        {
            filterModel.PageNumber = 3;
            filterModel.PageSize = 50;
            filterModel.TotalNumberOfRecords = 3 * filterModel.PageSize - 20;

            filterModel.PagedRecordsTo.Should().Be(filterModel.TotalNumberOfRecords);
        }
    }
}