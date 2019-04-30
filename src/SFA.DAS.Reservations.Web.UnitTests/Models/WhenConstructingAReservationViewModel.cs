using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    [TestFixture]
    public class WhenConstructingAReservationViewModel
    {
        [Test, AutoData]
        public void Then_Sets_Id(
            Reservation reservation)
        {
            var viewModel = new ReservationViewModel(reservation);

            viewModel.Id.Should().Be(reservation.Id);
        }

        [Test, AutoData]
        public void Then_Sets_StartDateDescription(
            Reservation reservation)
        {
            var viewModel = new ReservationViewModel(reservation);

            viewModel.StartDateDescription.Should().Be($"{reservation.StartDate:MMMM yyyy} to {reservation.ExpiryDate:MMMM yyyy}");
        }

        [Test, AutoData]
        public void Then_Sets_Status(
            Reservation reservation)
        {
            var viewModel = new ReservationViewModel(reservation);

            viewModel.Status.Should().Be(reservation.Status.ToString());
        }

        [Test, AutoData]
        public void Then_Sets_CourseDescription(
            Reservation reservation)
        {
            var viewModel = new ReservationViewModel(reservation);

            viewModel.CourseName.Should().Be(reservation.Course.CourseDescription);
        }

        [Test, AutoData]
        public void And_Course_Is_Null_Then_Sets_CourseDescription_To_Unknown(
            Reservation reservation)
        {
            reservation.Course = null;

            var viewModel = new ReservationViewModel(reservation);

            viewModel.CourseName.Should().Be("Unknown");
        }

        [Test, AutoData]
        public void Then_Sets_AccountLegalEntityName(
            Reservation reservation)
        {
            var viewModel = new ReservationViewModel(reservation);

            viewModel.LegalEntityName.Should().Be(reservation.AccountLegalEntityName);
        }
    }
}