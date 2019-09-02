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
            uint? loggedInProviderId)
        {
            var viewModel = new ReservationViewModel(reservation, url, loggedInProviderId);

            viewModel.Id.Should().Be(reservation.Id);
        }

        [Test, AutoData]
        public void Then_Sets_StartDateDescription(
            Reservation reservation,
            string url,
            uint? loggedInProviderId)
        {
            var viewModel = new ReservationViewModel(reservation, url, loggedInProviderId);

            viewModel.TrainingDate.StartDate.Should().Be(reservation.StartDate);
            viewModel.TrainingDate.EndDate.Should().Be(reservation.ExpiryDate);
        }

        [Test, AutoData]
        public void Then_Sets_Status(
            Reservation reservation,
            string url,
            uint? loggedInProviderId)
        {
            reservation.Status = ReservationStatus.Deleted;
            var viewModel = new ReservationViewModel(reservation, url, loggedInProviderId);

            ((int)viewModel.Status).Should().Be((int)reservation.Status);
        }

        [Test, AutoData]
        public void Then_Sets_CourseDescription(
            Reservation reservation,
            string url,
            uint? loggedInProviderId)
        {
            var viewModel = new ReservationViewModel(reservation, url, loggedInProviderId);

            viewModel.CourseName.Should().Be(reservation.Course.CourseDescription);
        }

        [Test, AutoData]
        public void And_Course_Is_Null_Then_Sets_CourseDescription_To_Unknown(
            Reservation reservation,
            string url,
            uint? loggedInProviderId)
        {
            reservation.Course = null;

            var viewModel = new ReservationViewModel(reservation, url, loggedInProviderId);

            viewModel.CourseName.Should().Be("Unknown");
        }

        [Test, AutoData]
        public void Then_Sets_AccountLegalEntityName(
            Reservation reservation,
            string url,
            uint? loggedInProviderId)
        {
            var viewModel = new ReservationViewModel(reservation, url, loggedInProviderId);

            viewModel.LegalEntityName.Should().Be(reservation.AccountLegalEntityName);
        }

        [Test, AutoData]
        public void Then_Inherits_From_AddApprenticeViewModel(
            ReservationViewModel viewModel)
        {
            Assert.IsInstanceOf<AddApprenticeViewModel>(viewModel);
        }

        [Test, AutoData]
        public void And_No_Ukprn_Then_Sets_CanBeDeleted_True(
            Reservation reservation,
            string url)
        {
            var viewModel = new ReservationViewModel(reservation, url, null);

            viewModel.CanBeDeleted.Should().Be(true);
        }

        [Test, AutoData]
        public void And_Has_Ukprn_And_RouteModel_Ukprn_Matches_Reservation_Id_Then_Sets_CanBeDeleted_True(
            Reservation reservation,
            string url)
        {
            var loggedInProviderId = reservation.ProviderId;

            var viewModel = new ReservationViewModel(reservation, url, loggedInProviderId);

            viewModel.CanBeDeleted.Should().Be(true);
        }

        [Test, AutoData]
        public void And_Has_Ukprn_And_RouteModel_Ukprn__Not_Match_Reservation_Id_Then_Sets_CanBeDeleted_False(
            Reservation reservation,
            string url,
            uint? loggedInProviderId)
        {
            var viewModel = new ReservationViewModel(reservation, url, loggedInProviderId);

            viewModel.CanBeDeleted.Should().Be(false);
        }
    }
}