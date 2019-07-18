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
            string accountHashedId,
            string accountLegalEntityPublicHashedId)
        {
            var viewModel = new ReservationViewModel(reservation, url, accountHashedId, accountLegalEntityPublicHashedId);

            viewModel.Id.Should().Be(reservation.Id);
        }

        [Test, AutoData]
        public void Then_Sets_StartDateDescription(
            Reservation reservation,
            string url,
            string accountHashedId,
            string accountLegalEntityPublicHashedId)
        {
            var viewModel = new ReservationViewModel(reservation, url, accountHashedId, accountLegalEntityPublicHashedId);

            viewModel.TrainingDate.StartDate.Should().Be(reservation.StartDate);
            viewModel.TrainingDate.EndDate.Should().Be(reservation.ExpiryDate);
        }

        [Test, AutoData]
        public void Then_Sets_Status(
            Reservation reservation,
            string url,
            string accountHashedId,
            string accountLegalEntityPublicHashedId)
        {
            reservation.Status = ReservationStatus.Deleted;
            var viewModel = new ReservationViewModel(reservation, url, accountHashedId, accountLegalEntityPublicHashedId);

            ((int)viewModel.Status).Should().Be((int)reservation.Status);
        }

        [Test, AutoData]
        public void Then_Sets_CourseDescription(
            Reservation reservation,
            string url,
            string accountHashedId,
            string accountLegalEntityPublicHashedId)
        {
            var viewModel = new ReservationViewModel(reservation, url, accountHashedId, accountLegalEntityPublicHashedId);

            viewModel.CourseName.Should().Be(reservation.Course.CourseDescription);
        }

        [Test, AutoData]
        public void And_Course_Is_Null_Then_Sets_CourseDescription_To_Unknown(
            Reservation reservation,
            string url,
            string accountHashedId,
            string accountLegalEntityPublicHashedId)
        {
            reservation.Course = null;

            var viewModel = new ReservationViewModel(reservation, url, accountHashedId, accountLegalEntityPublicHashedId);

            viewModel.CourseName.Should().Be("Unknown");
        }

        [Test, AutoData]
        public void Then_Sets_AccountLegalEntityName(
            Reservation reservation,
            string url,
            string accountHashedId,
            string accountLegalEntityPublicHashedId)
        {
            var viewModel = new ReservationViewModel(reservation, url, accountHashedId, accountLegalEntityPublicHashedId);

            viewModel.LegalEntityName.Should().Be(reservation.AccountLegalEntityName);
        }

        [Test, AutoData]
        public void Then_Inherits_From_AddApprenticeViewModel(
            ReservationViewModel viewModel)
        {
            Assert.IsInstanceOf<AddApprenticeViewModel>(viewModel);
        }

        [Test, AutoData]
        public void And_Has_Ukprn_And_ApprenticeUrl_Then_Provider_Url_Is_Created(
            Reservation reservation,
            string url,
            string accountHashedId,
            string accountLegalEntityPublicHashedId)
        {
            var viewModel = new ReservationViewModel(reservation, url, accountHashedId, accountLegalEntityPublicHashedId);

            Assert.AreEqual(
                $"{url}/{reservation.ProviderId}/unapproved/add-apprentice?reservationId={reservation.Id}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={reservation.StartDate:MMyyyy}&courseCode={reservation.Course.Id}", 
                viewModel.ApprenticeUrl);
        }

        [Test, AutoData]
        public void And_No_Ukprn_And_Has_ApprenticeUrl_Then_Employer_Url_Is_Created(
            Reservation reservation,
            string url,
            string accountHashedId,
            string accountLegalEntityPublicHashedId)
        {
            reservation.ProviderId = null;
            var viewModel = new ReservationViewModel(reservation, url, accountHashedId, accountLegalEntityPublicHashedId);

            Assert.AreEqual(
                $"{url}/{accountHashedId}/unapproved/add?reservationId={reservation.Id}&employerAccountLegalEntityPublicHashedId={accountLegalEntityPublicHashedId}&startMonthYear={reservation.StartDate:MMyyyy}&courseCode={reservation.Course.Id}", 
                viewModel.ApprenticeUrl);
        }
    }
}