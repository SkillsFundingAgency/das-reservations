using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenCastingManageReservationsFilterModelToDomainType
    {
        [Test, AutoData]
        public void Then_Sets_SearchTerm(
            ManageReservationsFilterModel filterModel)
        {
            ReservationFilter result = filterModel;

            result.SearchTerm.Should().Be(filterModel.SearchTerm);
            result.PageNumber.Should().Be(filterModel.PageNumber);
            result.PageSize.Should().Be(ManageReservationsFilterModel.PageSize);
            result.SelectedEmployer.Should().Be(filterModel.SelectedEmployer);
            result.SelectedCourse.Should().Be(filterModel.SelectedCourse);
            result.SelectedStartDate.Should().Be(filterModel.SelectedStartDate);
        }
    }
}