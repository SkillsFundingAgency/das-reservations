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
            Reservation reservation,
            string url,
            string alephid)
        {
            var viewModel = new ReservationViewModel(reservation, url, alephid);

            viewModel.Id.Should().Be(reservation.Id);
        }

        [Test, AutoData]
        public void Then_Sets_StartDateDescription(
            Reservation reservation,
            string url,
            string alephid)
        {
            var viewModel = new ReservationViewModel(reservation, url, alephid);

            viewModel.StartDateDescription.Should().Be($"{reservation.StartDate:MMMM yyyy} to {reservation.ExpiryDate:MMMM yyyy}");
        }

        [Test, AutoData]
        public void Then_Sets_Status(
            Reservation reservation,
            string url,
            string alephid)
        {
            reservation.Status = ReservationStatus.Deleted;
            var viewModel = new ReservationViewModel(reservation, url, alephid);

            ((int)viewModel.Status).Should().Be((int)reservation.Status);
        }

        [Test, AutoData]
        public void Then_Sets_CourseDescription(
            Reservation reservation,
            string url,
            string alephid)
        {
            var viewModel = new ReservationViewModel(reservation, url, alephid);

            viewModel.CourseName.Should().Be(reservation.Course.CourseDescription);
        }

        [Test, AutoData]
        public void And_Course_Is_Null_Then_Sets_CourseDescription_To_Unknown(
            Reservation reservation,
            string url,
            string alephid)
        {
            reservation.Course = null;

            var viewModel = new ReservationViewModel(reservation, url, alephid);

            viewModel.CourseName.Should().Be("Unknown");
        }

        [Test, AutoData]
        public void Then_Sets_AccountLegalEntityName(
            Reservation reservation,
            string url,
            string alephid)
        {
            var viewModel = new ReservationViewModel(reservation, url, alephid);

            viewModel.LegalEntityName.Should().Be(reservation.AccountLegalEntityName);
        }

        [Test, AutoData]
        public void Then_Inherits_From_AddApprenticeViewModel(
            ReservationViewModel viewModel)
        {
            Assert.IsInstanceOf<AddApprenticeViewModel>(viewModel);
        }

        [Test, AutoData]
        public void Then_If_The_ApprenticeUrl_Is_Available_The_Url_Is_Created_If_Supplied(
            Reservation reservation,
            string url,
            string alephid)
        {
            var viewModel = new ReservationViewModel(reservation, url, alephid);

            Assert.AreEqual($"{url}/{reservation.ProviderId}/unapproved/add-apprentice?reservationId={reservation.Id}&employerAccountLegalEntityPublicHashedId={alephid}&startMonthYear={reservation.StartDate:MMyyyy}&courseCode={reservation.Course.Id}", viewModel.ApprenticeUrl);
        }
    }
}